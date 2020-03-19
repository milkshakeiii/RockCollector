from django.http import HttpResponse
from django.contrib.auth import authenticate, login
from django.contrib.auth.models import User
from webapi.game import make_game


AUTH_FAILED_MESSAGE = "Authentication failed."


def one_message_authenticate(request):
    username = request.GET['username']
    password = request.GET['password']
    user = authenticate(request, username=username, password=password)
    if not User.objects.filter(username=username).exists():
        user = User.objects.create_user(username, 'noemail@noemail.com', password)
    if user is not None:
        login(request, user)
    return user


def index(request):
    user = one_message_authenticate(request)
    if user is not None:
        return HttpResponse("Authenticated successfully: " + user.username)
    else:
        return HttpResponse(AUTH_FAILED_MESSAGE)


users_looking_for_games = []
def find_game(request):
    user = one_message_authenticate(request)
    if user is None:
        return HttpResponse(AUTH_FAILED_MESSAGE)
    
    if (len(users_looking_for_games) > 0):
        opponent = users_looking_for_games.pop()
        return HttpResponse(make_game(user, opponent))
    else:
        users_looking_for_games.append(user)
        return HttpResponse("Added to queue")


def make_move(request):
    user = one_message_authenticate(request)
    if user is None:
        return HttpResponse(AUTH_FAILED_MESSAGE)
    else:
        return HttpResponse("So, you want to make a move...")
