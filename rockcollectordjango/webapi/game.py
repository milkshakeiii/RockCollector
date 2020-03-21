from webapi.models import Gamestate
from enum import Enum
import uuid



#######################
class BoardTiles(Enum):
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
    #rock numbers are concatenated digits corresponding to color, shape, material enums
    #example: 001001001 is GREY CIRCULAR KINGSTONE
    def get_rock_number(self):
        color_string = str(int(self.color))
        shape_string = str(int(self.shape))
        material_string = str(int(self.material))
        for number_string in [color_string, shape_string, material_string]:
            if (len(number_string) <= 0 or len(number_string) > 3):
                raise Exception("Invalid string from rock enum.  Too long?")
        color_string = (3-len(color_string))*"0" + color_string
        shape_string = (3-len(shape_string))*"0" + shape_string
        material_string = (3-len(color_string))*"0" + material_string
        return int(color_string+shape_string+material_string)
    def get_name_string(self):
        return self.color.name + " " + self.shape.name + " " + self.material.name
    def rock_from_rock_number(rock_number):
        rock_string = str(rock_number)
        color = int(rock_string[0:3])
        shape = int(rock_string[3:6])
        material = int(rock_string[6:9])
        return Rock(color, shape, material)
        
#######################



def make_game(player1, player2): #player1, player2 are django users
    first_gamestate = Gamestate(player1_user=player1, player2_user=player2)
    first_gamestate.game_id = uuid.uuid4
    first_gamestate.board_width = 8
    first_gamestate.board_height = 8
    first_gamestate.location = Locations.JUNGLE

    first_gamestate.board_tiles = ([BoardTiles.PLAIN_GROUND] *
                                    first_gamestate.board_width *
                                    first_gamestate.board_height)
    
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

    first_gamestate.player1_rocks_x_coords = [-1] * 8
    first_gamestate.player1_rocks_y_coords = [-1] * 8
    first_gamestate.player2_rocks_x_coords = [-2] * 8
    first_gamestate.player2_rocks_y_coords = [-2] * 8
    
    return first_gamestate
