from django.urls import path

from . import views

urlpatterns = [
    path('', views.index),
    path('find_game', views.find_game),
    path('make_move', views.make_move),
    path('check_for_gamestate', views.check_for_gamestate),
]
