from django.http import HttpResponse
from rest_framework.authtoken.models import Token
from rest_framework.response import Response
from rest_framework.decorators import api_view
from django.contrib.auth.models import User

import numbers
import httpx

import playapp.models as models


def index(request):
    return HttpResponse("Hello, world. You're at the game index.")


def validations(request, parameters):
    """
    helper function to validate request.
    """
    if request.method != 'POST':
        # return error response
        return Response(status=405, content="Only POST requests are allowed.")
    
    if request.user.is_anonymous:
        # return error response
        return Response(status=401, content="User is not authenticated.")

    for name, type in parameters.items():
        if name not in request.data:
            return error_response("Missing required field: " + name)
        if not isinstance(request.data[name], type):
            return error_response("Invalid type for field " + name + ". Expected " + str(type) + ", got " + str(request.data[name]) + ".")
        if type == list:
            # make sure all items are strings
            for item in request.data[name]:
                if not isinstance(item, str):
                    return error_response("Invalid type for field " + name + ". Expected list of strings, got list of " + str(item) + ".")
    
    return None


def error_response(message):
    """
    helper function to return error response.
    """
    # serialize the error message into a JSON object
    error = {
        'error': message
    }

    # return the error response
    return Response(error)


def success_response(message):
    """
    helper function to return success response.
    """
    # serialize the success message into a JSON object
    success = {
        'success': message
    }

    # return the success response
    return Response(success)


@api_view(['POST'])
def users_authenticate_or_create_from_steam(request):
    """
    View for authenticating a user from a steamworks AuthTicketForWebApi.

    Request body should be a JSON object with the following fields:
    - steam_auth_ticket (string): the steamworks AuthTicketForWebApi hexadecimal string

    Returns:
    - the auth token for the user if the user is authenticated or created successfully
    """
    # parse request body
    data = request.data
    if 'steam_auth_ticket' not in data:
        return error_response("Missing required field: steam_auth_ticket")
    steam_auth_ticket = data['steam_auth_ticket']
    if not isinstance(steam_auth_ticket, str):
        return error_response("Invalid type for field steam_auth_ticket. Expected string, got " + str(type(steam_auth_ticket)) + ".")

    # request data
    steam_web_api_key = "BB568C35235E8D2799FB20985A94F5C2"
    steam_app_id = 1309380

    # make an HTTPS request to partner.steam-api.com and call the ISteamUserAuth/AuthenticateUserTicket web method, 
    # passing the user's session ticket as a hexadecimal string
    url = "https://api.steampowered.com/ISteamUserAuth/AuthenticateUserTicket/v1/"
    params = {
        "key": steam_web_api_key,
        "appid": steam_app_id,
        "ticket": steam_auth_ticket,
        "identity": "exodivers"
    }
    response = httpx.get(url, params=params)
    if response.status_code != 200:
        return error_response("Steamworks API request failed: " + response.text)
    response_data = response.json()
    if 'response' not in response_data:
        return error_response("Steamworks API response does not contain 'response' field.")
    if 'error' in response_data['response']:
        return error_response("Steamworks API response contains error: " + str(response_data['response']['error']))
    if 'params' not in response_data['response']:
        return error_response("Steamworks API response does not contain 'params' field.")
    if 'steamid' not in response_data['response']['params']:
        return error_response("Steamworks API response does not contain 'result' field.")
    steam_id = response_data['response']['params']['steamid']

    # check if the user already exists
    try:
        user = User.objects.get(username=steam_id)
    except User.DoesNotExist:
        # create the user
        user = User(username=steam_id)
        user.save()

    # get the auth token
    auth_token, _ = Token.objects.get_or_create(user=user)
    auth_token.save()

    # return the auth token
    return Response({'token': auth_token.key})


@api_view(['POST'])
def report_score(request):
    """
    View for reporting a score for a user.

    Request body should be a JSON object with the following fields:
    - groups (list of strings): the competition groups the score counts for
    - one entry each for each score type to be reported, values should be floats or integers. example:
        - "points": 100
        - "time": 60.0
        - "enemies_defeated": 15

    Returns:
    - a success message if the score is reported successfully
    """
    # validate request
    error = validations(request, {'groups': list})
    if error:
        return error
    
    # expect at least one score field
    if len(request.data) < 2:
        return error_response("At least one score field is required.")

    # validate score fields
    for key, value in request.data.items():
        if key != 'groups' and not isinstance(value, numbers.Number):
            return error_response("Invalid type for field " + key + ". Expected number, got " + str(value))

    # get the user
    user = request.user

    # get the groups
    groups = request.data['groups']

    # get the scores
    scores = request.data
    del scores['groups']

    # get the group objects
    group_objects = []
    for group_name in groups:
        category, _ = models.Group.objects.get_or_create(name=group_name)
        group_objects.append(category)

    # create the score objects
    for score_type, value in scores.items():
        score = models.Score(user=user, score_type=score_type, value=value)
        score.save()
        score.groups.set(group_objects)

    # for each group and each score type, find the number of scores in the group that are greater than the user's score
    place_rankings = {}
    for group in group_objects:
        place_rankings[group.name] = {}
        for score_type in scores:
            place_rankings[group.name][score_type] = models.Score.objects.filter(groups=group, score_type=score_type, value__gt=value).count()

    # return place rankings
    return Response(place_rankings)


@api_view(['POST'])
def get_scores(request):
    """
    View for retrieving scores by group, score type, and ranking position either absolute or relative to the user's ranking.

    Request query parameters:
    - groups (list of strings): retrive scores that belong to every group in the list (AND relationship)
    - score_type (string): the type of score to retrieve
    - count (int): the number of scores to retrieve
    - start (int, optional): the ranking position to start from
    - around_user (bool, optional): if true, instead retrieve scores around the user's ranking position

    Returns:
    - a JSON object with the scores for the specified group, score type, and ranking position
    """
    # validate request
    error = validations(request, {'groups': list, 'score_type': str, 'count': int})
    if error:
        return error
    
    # exactly one of start and around_user must be provided
    if 'start' in request.data == 'around_user' in request.data:
        return error_response("Exactly one of start and around_user must be provided.")

    # get the groups
    groups = request.data['groups']

    # get the score type
    score_type = request.data['score_type']

    # get the count
    count = request.data['count']

    # get the start
    start = request.data.get('start', 0)
    if not isinstance(start, int):
        return error_response("Invalid type for field start. Expected int, got " + str(start))

    # get the around_user
    around_user = request.data.get('around_user', False)
    if not isinstance(around_user, bool):
        return error_response("Invalid type for field around_user. Expected bool, got " + str(around_user))

    # get the user
    user = request.user

    # get the group objects
    group_objects = []
    for group_name in groups:
        category, _ = models.Group.objects.get_or_create(name=group_name)
        group_objects.append(category)

    # get the scores
    scores = models.Score.objects.filter(groups__in=group_objects, score_type=score_type).order_by('-value')

    # get the scores around the user's ranking if requested
    if around_user:
        # get the user's score and get the ranking position of the highest score
        user_score = scores.filter(user=user).first()
        user_ranking = scores.filter(value__gt=user_score.value).count()
        start = max(0, user_ranking - count // 2)
    
    # get the scores from the determined start position
    scores = scores[start:start + count]

    # serialize the scores
    scores_data = []
    for score in scores:
        scores_data.append({
            'user': score.user.username,
            'value': score.value
        })

    # return the scores
    return Response(scores_data)


@api_view(['POST'])
def get_market_prices(request):
    """
    View for retrieving market prices for a list of items.

    Request body should be a JSON object with the following fields:
    - items (list of strings): the list of items to retrieve prices for
    - price_groups (list of strings): the price groups for the items

    Returns:
    - a JSON object with the prices for the specified items
    """
    # validate request
    error = validations(request, {'items': list, 'price_groups': list})
    if error:
        return error
    
    # get the items
    items = request.data['items']

    # get the price groups
    price_groups = request.data.get('price_groups', [])

    # require itmes and price groups to be the same length
    if len(items) != len(price_groups):
        return error_response("items and price_groups must be the same length.")

    # get the item objects
    item_objects = []
    for item_name in items:
        item, _ = models.Item.objects.get_or_create(name=item_name)
        item_objects.append(item)

    # get the price group objects
    price_group_objects = []
    for price_group_name in price_groups:
        price_group, _ = models.PriceGroup.objects.get_or_create(name=price_group_name)
        price_group_objects.append(price_group)

    # get the prices (sum of item price and price group price)
    prices_data = {}
    for item, group in zip(item_objects, price_group_objects):
        prices_data[item.name] = item.price + group.price_modifier

    # return the prices
    return Response(prices_data)


@api_view(['POST'])
def report_purchase(request):
    """
    View for reporting a purchase of an item.

    Request body should be a JSON object with the following fields:
    - item (string): the name of the item that was purchased
    - price_group (string): the price group the item was purchased from
    - price_adjustment (float or int): increase the price of the item by this amount
        - and decrease the price of the price group by this amount divided by
        - the number of items in the price group

    Returns:
    - the new price of the item and the price group modifier
    """
    # validate request
    error = validations(request, {'item': str, 'price_group': str, 'price_adjustment': numbers.Number})
    if error:
        return error
    
    # get the price adjustment
    price_adjustment = request.data['price_adjustment']

    # get the item
    item_name = request.data['item']
    item, _ = models.Item.objects.get_or_create(name=item_name)

    # get the price group and make sure the item is in the price group
    price_group_name = request.data['price_group']
    price_group, _ = models.PriceGroup.objects.get_or_create(name=price_group_name)
    price_group.items.add(item)

    # count the number of items in the price group
    num_items = price_group.items.count()

    # adjust the price of the item
    item.price += price_adjustment

    # adjust the price_modifier of the price group
    price_modifier_change = -price_adjustment / num_items
    price_group.price_modifier += price_modifier_change

    # save the changes
    item.save()
    price_group.save()

    # return the price modifier change in case the client wants to update the prices
    return Response({
        item_name: item.price,
        price_group_name: price_group.price_modifier,
        'price_modifier_change': price_modifier_change
    })