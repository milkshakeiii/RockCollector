from django.db import models
from enum import Enum
from django.core.validators import int_list_validator

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
    

class Gamestate(models.Model):
    board_width = models.IntegerField(default=0)
    board_height = models.IntegerField(default=0)
    board_tiles = models.CharField(validators=[int_list_validator],
                                   max_length=1000)
