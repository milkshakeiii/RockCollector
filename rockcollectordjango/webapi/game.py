from webapi.models import Gamestate
from enum import Enum
import uuid



#######################
class BoardTile(Enum):
    PLAIN_GROUND = 1
    VOID = 2
class Colors(Enum):
    GREY = 1
    BLUE = 2
    YELLOW = 3
class Shapes(Enum):
    CIRCULAR = 1
    SQUARE = 2
    TRIANGULAR = 3
    STARSHAPED = 4
    LUMPY = 5
    HOOKED = 6
class Materials(Enum):
    KINGSTONE = 1
    SHALE = 2
class Locations(Enum):
    JUNGLE = 1

class Rock():
    def __init__(self, color, shape, material):
        self.color = color
        self.shape = shape
        self.material = material
#######################



def make_game(player1, player2): #player1, player2 are django users
    first_gamestate = Gamestate(player1_user=player1, player2_user=player2)
    first_gamestate.game_id = uuid.uuid4
    first_gamestate.player1_rocks = [Rock(Colors.GREY, Shapes.SQUARE, Materials.SHALE),
                                     Rock(Colors.GREY, Shapes.HOOKED, Materials.SHALE),
                                     Rock(Colors.GREY, Shapes.TRIANGULAR, Materials.SHALE),
                                     Rock(Colors.GREY, Shapes.STARSHAPED, Materials.SHALE),
                                     Rock(Colors.GREY, Shapes.CIRCULAR, Materials.SHALE),
                                     Rock(Colors.GREY, Shapes.TRIANGULAR, Materials.SHALE),
                                     Rock(Colors.GREY, Shapes.HOOKED, Materials.SHALE),
                                     Rock(Colors.GREY, Shapes.SQUARE, Materials.SHALE)]
    first_gamestate.player2_rocks = [Rock(Colors.GREY, Shapes.SQUARE, Materials.SHALE),
                                     Rock(Colors.GREY, Shapes.HOOKED, Materials.SHALE),
                                     Rock(Colors.GREY, Shapes.TRIANGULAR, Materials.SHALE),
                                     Rock(Colors.GREY, Shapes.STARSHAPED, Materials.SHALE),
                                     Rock(Colors.GREY, Shapes.CIRCULAR, Materials.SHALE),
                                     Rock(Colors.GREY, Shapes.TRIANGULAR, Materials.SHALE),
                                     Rock(Colors.GREY, Shapes.HOOKED, Materials.SHALE),
                                     Rock(Colors.GREY, Shapes.SQUARE, Materials.SHALE)]
    
    return first_gamestate
