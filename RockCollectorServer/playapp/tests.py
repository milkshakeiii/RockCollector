from django.test import TestCase
from rest_framework.test import APIRequestFactory
import datetime
import time

import django.contrib.auth.models as auth_models
import rest_framework.authtoken.views as auth_views
import playapp.models as models
import playapp.views as views


class TestReportScores(TestCase):
    def setUp(self):
        # create test user
        user = auth_models.User.objects.create_user('test', password='test')
        user.save()

        # use auth with username and password to get a token
        self.factory = APIRequestFactory()
        request = self.factory.post('unused', {'username': 'test', 'password': 'test'}, format='json')
        response = auth_views.obtain_auth_token(request)
        self.token = response.data['token']

    def test_report_score(self):
        # report a score
        request = self.factory.post('unused', {'groups': ['test_group'], 'points': 100}, format='json', HTTP_AUTHORIZATION=f'Token {self.token}')
        response = views.report_score(request)
        data = response.data
        assert data == {'test_group': {'points': 1}}

        request = self.factory.post('unused', {'groups': ['test_group', 'test_group_2'], 'points': 50}, format='json', HTTP_AUTHORIZATION=f'Token {self.token}')
        response = views.report_score(request)
        data = response.data
        assert data == {'test_group': {'points': 2}, 'test_group_2': {'points': 1}}