from django.http import HttpResponse
from rest_framework.authtoken.models import Token
from rest_framework.parsers import JSONParser
from rest_framework.response import Response
from rest_framework.decorators import api_view
from django.contrib.auth.models import User

import time
import datetime
import json
import httpx


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