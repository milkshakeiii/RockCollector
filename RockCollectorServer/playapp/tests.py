from django.test import TestCase
from rest_framework.test import APIRequestFactory
import datetime
import time

import django.contrib.auth.models as auth_models
import rest_framework.authtoken.views as auth_views
import playapp.models as models
import playapp.views as views


class TestHighScores(TestCase):
    def setUp(self):
        self.factory = APIRequestFactory()

        # create test user
        user = auth_models.User.objects.create_user('test', password='test')
        user.save()

        # use auth with username and password to get a token
        request = self.factory.post('unused', {'username': 'test', 'password': 'test'}, format='json')
        response = auth_views.obtain_auth_token(request)
        self.token1 = response.data['token']

        # create second user
        user = auth_models.User.objects.create_user('test2', password='test2')
        user.save()

        # use auth with username and password to get a token
        request = self.factory.post('unused', {'username': 'test2', 'password': 'test2'}, format='json')
        response = auth_views.obtain_auth_token(request)
        self.token2 = response.data['token']

    def test_report_score(self):
        # report a score
        request = self.factory.post('unused', {'groups': ['test_group'], 'points': 100}, format='json', HTTP_AUTHORIZATION=f'Token {self.token1}')
        response = views.report_score(request)
        data = response.data
        assert data == {'test_group': {'points': 0}}

        request = self.factory.post('unused', {'groups': ['test_group', 'test_group_2'], 'points': 50}, format='json', HTTP_AUTHORIZATION=f'Token {self.token1}')
        response = views.report_score(request)
        data = response.data
        assert data == {'test_group': {'points': 1}, 'test_group_2': {'points': 0}}

    def test_get_scores(self):
        # report 6 scores
        request = self.factory.post('unused', {'groups': ['test_group'], 'points': 100}, format='json', HTTP_AUTHORIZATION=f'Token {self.token1}')
        response = views.report_score(request)
        request = self.factory.post('unused', {'groups': ['test_group'], 'points': 90}, format='json', HTTP_AUTHORIZATION=f'Token {self.token1}')
        response = views.report_score(request)
        request = self.factory.post('unused', {'groups': ['test_group'], 'points': 80}, format='json', HTTP_AUTHORIZATION=f'Token {self.token1}')
        response = views.report_score(request)
        request = self.factory.post('unused', {'groups': ['test_group'], 'points': 70}, format='json', HTTP_AUTHORIZATION=f'Token {self.token2}')
        response = views.report_score(request)
        request = self.factory.post('unused', {'groups': ['test_group'], 'points': 60}, format='json', HTTP_AUTHORIZATION=f'Token {self.token1}')
        response = views.report_score(request)
        request = self.factory.post('unused', {'groups': ['test_group'], 'points': 50}, format='json', HTTP_AUTHORIZATION=f'Token {self.token2}')
        response = views.report_score(request)

        # get the score
        request = self.factory.post('unused', {
            "groups": ["test_group"],
            "score_type": "points",
            "count": 16,
            "start": 0,
        }, format='json', HTTP_AUTHORIZATION=f'Token {self.token1}')
        response = views.get_scores(request)
        data = response.data
        assert data == [
            {'user': 'test', 'value': 100.0},
            {'user': 'test', 'value': 90.0},
            {'user': 'test', 'value': 80.0},
            {'user': 'test2', 'value': 70.0},
            {'user': 'test', 'value': 60.0},
            {'user': 'test2', 'value': 50.0}
        ]

        # get scores using around_user
        request = self.factory.post('unused', {
            "groups": ["test_group"],
            "score_type": "points",
            "count": 3,
            "around_user": True,
        }, format='json', HTTP_AUTHORIZATION=f'Token {self.token2}')
        response = views.get_scores(request)
        data = response.data
        assert data == [
            {'user': 'test', 'value': 80.0},
            {'user': 'test2', 'value': 70.0},
            {'user': 'test', 'value': 60.0}
        ]