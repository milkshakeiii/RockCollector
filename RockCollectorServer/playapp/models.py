from django.db import models


### MODELS FOR HIGH SCORES ###
class Group(models.Model):
    created = models.DateTimeField(auto_now_add=True)

    name = models.CharField(max_length=100)

    def __str__(self):
        return self.name

class Score(models.Model):
    created = models.DateTimeField(auto_now_add=True)

    user = models.ForeignKey('auth.User', on_delete=models.CASCADE)
    score_type = models.CharField(max_length=100)
    value = models.FloatField()
    groups = models.ManyToManyField(Group, related_name='scores')
### END MODELS FOR HIGH SCORES ###


### MODELS FOR MARKET ###
class Item(models.Model):
    created = models.DateTimeField(auto_now_add=True)

    name = models.CharField(max_length=100)
    price = models.FloatField(default=0)


class PriceGroup(models.Model):
    created = models.DateTimeField(auto_now_add=True)

    name = models.CharField(max_length=100)
    items = models.ManyToManyField(Item, related_name='price_groups')
    price_modifier = models.FloatField(default=0)
### END MODELS FOR MARKET ###