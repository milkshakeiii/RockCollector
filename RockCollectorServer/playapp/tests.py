from django.test import TestCase
from rest_framework.test import APIRequestFactory
import datetime
import time

import django.contrib.auth.models as auth_models
import rest_framework.authtoken.views as auth_views
import playapp.models as models
import playapp.views as views


class TestPlayApp(TestCase):
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

    def test_get_market_prices(self):
        # create items
        item1 = models.Item.objects.create(name='item1', price=100)
        item2 = models.Item.objects.create(name='item2', price=200)
        item3 = models.Item.objects.create(name='item3', price=300)

        # create price groups
        price_group1 = models.PriceGroup.objects.create(name='price_group1', price_modifier=-50)
        price_group1.items.add(item1, item2)
        price_group2 = models.PriceGroup.objects.create(name='price_group2', price_modifier=-60)
        price_group2.items.add(item2, item3)

        # get the prices
        request = self.factory.post('unused', {
            "price_groups": ["price_group1", "price_group2", "price_group2"],
            "items": ["item1", "item2", "item3"],
        }, format='json', HTTP_AUTHORIZATION=f'Token {self.token1}')
        response = views.get_market_prices(request)
        data = response.data
        assert data == {
            'item1': 50.0,
            'item2': 140.0,
            'item3': 240.0,
        }

    def test_report_purchase(self):
        # report a purchase
        request = self.factory.post('unused', {
            "item": "item1",
            "price_group": "price_group1",
            "price_adjustment": 1,
        }, format='json', HTTP_AUTHORIZATION=f'Token {self.token1}')
        response = views.report_purchase(request)
        data = response.data
        assert data == {
            'item1': 1,
            'price_group1': -1,
            'price_modifier_change': -1,   
        }

        # report a second purchase in the same price group
        request = self.factory.post('unused', {
            "item": "item2",
            "price_group": "price_group1",
            "price_adjustment": 1,
        }, format='json', HTTP_AUTHORIZATION=f'Token {self.token1}')
        response = views.report_purchase(request)
        data = response.data
        assert data == {
            'item2': 1,
            'price_group1': -1.5,
            'price_modifier_change': -0.5,
        }

        # buy the first item again
        request = self.factory.post('unused', {
            "item": "item1",
            "price_group": "price_group1",
            "price_adjustment": 1,
        }, format='json', HTTP_AUTHORIZATION=f'Token {self.token1}')
        response = views.report_purchase(request)
        data = response.data
        assert data == {
            'item1': 2,
            'price_group1': -2,
            'price_modifier_change': -0.5,
        }