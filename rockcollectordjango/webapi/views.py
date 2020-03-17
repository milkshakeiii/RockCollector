from django.http import HttpResponse
from django.contrib.auth import authenticate, login
from django.contrib.auth.models import User

def index(request):
    username = request.GET['username']
    password = request.GET['password']
    user = authenticate(request, username=username, password=password)
    if not User.objects.filter(username=username).exists():
        user = User.objects.create_user(username, 'noemail@noemail.com', password)
    if user is not None:
        login(request, user)
        return HttpResponse("Success.")
    else:
        return HttpResponse("Invalid login, yo.")
