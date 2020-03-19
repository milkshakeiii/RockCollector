from django.db import models
from enum import Enum
from django.core.validators import int_list_validator
from django.contrib.auth.models import User
from django.db.models.signals import post_save
from django.dispatch import receiver


#######################
class BoardTile(Enum):
    PLAIN_GROUND = 1
    VOID = 2
class Colors(Enum):
    GREY = 1
    BLUE = 2
    YELLOW = 3
class Shapes(Enum):
    CIRCLULAR = 1
    SQUARE = 2
    TRIANGULAR = 3
    STARSHAPED = 4
    LUMPY = 5
    HOOKED = 6
class Materials(Enum):
    KINGSTONE = 1
#######################


###ONE-TO-ONE PROFILE WITH USER###
class Profile(models.Model):
    user = models.OneToOneField(User, on_delete=models.CASCADE)
    collection = models.CharField(validators=[int_list_validator],
                                  max_length=10000)


@receiver(post_save, sender=User)
def create_user_profile(sender, instance, created, **kwargs):
    if created:
        Profile.objects.create(user=instance)


@receiver(post_save, sender=User)
def save_user_profile(sender, instance, **kwargs):
    instance.profile.save()
##################################


#main game models
class Gamestate(models.Model):
    game_number = models.IntegerField(default=0)
    board_width = models.IntegerField(default=0)
    board_height = models.IntegerField(default=0)
    player1_active = models.BooleanField(default=True)
    board_tiles = models.CharField(validators=[int_list_validator],
                                   max_length=1000)
    player1_rocks = models.CharField(validators=[int_list_validator],
                                     max_length=8)
    player2_rocks = models.CharField(validators=[int_list_validator],
                                     max_length=8)
    player1_rock_x_coords = models.CharField(validators=[int_list_validator],
                                             max_length=8)
    player1_rock_y_coords = models.CharField(validators=[int_list_validator],
                                             max_length=8)
    player2_rock_x_coords = models.CharField(validators=[int_list_validator],
                                             max_length=8)
    player2_rock_y_coords = models.CharField(validators=[int_list_validator],
                                             max_length=8)
    player1_user = models.ForeignKey(User,
                                     null=True,
                                     on_delete=models.SET_NULL,
                                     related_name='all_player1_gamestates')
    player2_user = models.ForeignKey(User,
                                     null=True,
                                     on_delete=models.SET_NULL,
                                     related_name='all_player2_gamestates')
