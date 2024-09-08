from django.urls import path, include

from rest_framework.authtoken import views

urlpatterns = [
    path('api-token-auth/', views.obtain_auth_token),
    path('', include('helloapp.urls')),
]
