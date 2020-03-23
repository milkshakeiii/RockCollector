from django.db import models
from django.core.validators import int_list_validator
from django.contrib.auth.models import User
from django.db.models.signals import post_save
from django.dispatch import receiver



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
    game_uuid = models.UUIDField()
    
    #how many turns have ALREADY been taken
    turns_taken = models.IntegerField(default=0)
    #whether a player has already submitted a valid move to make in this state
    valid_action_submitted = models.BooleanField(default=False)
    
    board_width = models.IntegerField(default=0)
    board_height = models.IntegerField(default=0)
    location = models.IntegerField(default=0) #see location Enum in game.py
    game_over = models.BooleanField(default=False)
    player1_wins = models.BooleanField(default=False)
    
    #these are rock numbers, see Rock in game.py
    rocks_awarded = models.CharField(validators=[int_list_validator],
                                     max_length=100)
    
    board_tiles = models.CharField(validators=[int_list_validator],
                                   max_length=1000)
    
    #more rock numbers
    player1_rocks = models.CharField(validators=[int_list_validator],
                                     max_length=80)
    player2_rocks = models.CharField(validators=[int_list_validator],
                                     max_length=80)

    #(-1, -1) for player 1 ready zone, (-2, -2) for player 2 ready zone
    #(-3, -3) for pieces captured by player 1
    #(-4, -4) for pieces captured by player 2
    #(-5, -5) for destroyed pieces
    player1_rocks_x_coords = models.CharField(validators=[int_list_validator(allow_negative=True)],
                                             max_length=80,)
    player1_rocks_y_coords = models.CharField(validators=[int_list_validator(allow_negative=True)],
                                             max_length=80,)
    player2_rocks_x_coords = models.CharField(validators=[int_list_validator(allow_negative=True)],
                                             max_length=80,)
    player2_rocks_y_coords = models.CharField(validators=[int_list_validator(allow_negative=True)],
                                             max_length=80,)
    
    player1_user = models.ForeignKey(User,
                                     null=True,
                                     on_delete=models.SET_NULL,
                                     related_name='all_player1_gamestates')
    player2_user = models.ForeignKey(User,
                                     null=True,
                                     on_delete=models.SET_NULL,
                                     related_name='all_player2_gamestates')
