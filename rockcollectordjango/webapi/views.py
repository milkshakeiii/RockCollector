from django.http import HttpResponse
from django.contrib.auth import authenticate, login

def index(request):
    username = request.GET['username']
    password = request.GET['password']
    user = authenticate(request, username=username, password=password)
    if user is not None:
        login(request, user)
        return HttpResponse("Success.")
    else:
        return HttpResponse("Invalid login, yo.")
