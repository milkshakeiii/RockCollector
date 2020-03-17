from django.http import HttpResponse
from django.contrib.auth import authenticate, login
from django.contrib.auth.models import User


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


def find_game(request):
    user = one_message_authenticate(request)
    if user is None:
        return HttpResponse(AUTH_FAILED_MESSAGE)
    else:
        return HttpResponse("So, you want to find a game...")


def make_move(request):
    user = one_message_authenticate(request)
    if user is None:
        return HttpResponse(AUTH_FAILED_MESSAGE)
    else:
        return HttpResponse("So, you want to make a move...")
