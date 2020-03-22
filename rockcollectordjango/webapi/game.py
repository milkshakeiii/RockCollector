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
    def get_name_string(self):
        return self.color.name + " " + self.shape.name + " " + self.material.name
    #rock numbers are concatenated digits corresponding to color, shape, material enums
    #which are then reversed
    #example: 200100100 is GREY CIRCULAR SHALE
    #but NO ONE SHOULD HAVE TO KNOW THIS except the following methods
    def get_rock_number(self):
        color_string = str(int(self.color.value))
        shape_string = str(int(self.shape.value))
        material_string = str(int(self.material.value))
        for number_string in [color_string, shape_string, material_string]:
            if (len(number_string) <= 0 or len(number_string) > 3):
                raise Exception("Invalid string from rock enum.  Too long?")
        len3_color_string = (3-len(color_string))*"0" + color_string
        len3_shape_string = (3-len(shape_string))*"0" + shape_string
        len3_material_string = (3-len(material_string))*"0" + material_string
        for len3_string in [len3_color_string, len3_shape_string, len3_material_string]:
            if (len(len3_string) != 3) or int(len3_string) == 0:
                raise Exception("Invalid result from get_rock_number")
        return int(''.join(reversed(len3_color_string+len3_shape_string+len3_material_string)))
    def rock_from_rock_number(rock_number):
        rock_string = ''.join(reversed(str(rock_number)))
        color = Colors(int(rock_string[0:3]))
        shape = Shapes(int(rock_string[3:6]))
        material = Materials(int(rock_string[6:9]))
        return Rock(color, shape, material)
        
#######################



def make_game(player1, player2): #player1, player2 are django users
    first_gamestate = Gamestate(player1_user=player1, player2_user=player2)
    first_gamestate.game_id = uuid.uuid4()
    first_gamestate.board_width = 8
    first_gamestate.board_height = 8
    first_gamestate.location = Locations.JUNGLE.value

    first_gamestate.board_tiles = ([BoardTiles.PLAIN_GROUND.value] *
                                    first_gamestate.board_width *
                                    first_gamestate.board_height)
    
    player1_rocks_int_list = [Rock(Colors.GREY, Shapes.SQUARE, Materials.SHALE).get_rock_number(),
                              Rock(Colors.GREY, Shapes.HOOKED, Materials.SHALE).get_rock_number(),
                              Rock(Colors.GREY, Shapes.TRIANGULAR, Materials.SHALE).get_rock_number(),
                              Rock(Colors.GREY, Shapes.STARSHAPED, Materials.SHALE).get_rock_number(),
                              Rock(Colors.GREY, Shapes.CIRCULAR, Materials.SHALE).get_rock_number(),
                              Rock(Colors.GREY, Shapes.TRIANGULAR, Materials.SHALE).get_rock_number(),
                              Rock(Colors.GREY, Shapes.HOOKED, Materials.SHALE).get_rock_number(),
                              Rock(Colors.GREY, Shapes.SQUARE, Materials.SHALE).get_rock_number()]
    player2_rocks_int_list = [Rock(Colors.GREY, Shapes.SQUARE, Materials.SHALE).get_rock_number(),
                              Rock(Colors.GREY, Shapes.HOOKED, Materials.SHALE).get_rock_number(),
                              Rock(Colors.GREY, Shapes.TRIANGULAR, Materials.SHALE).get_rock_number(),
                              Rock(Colors.GREY, Shapes.STARSHAPED, Materials.SHALE).get_rock_number(),
                              Rock(Colors.GREY, Shapes.CIRCULAR, Materials.SHALE).get_rock_number(),
                              Rock(Colors.GREY, Shapes.TRIANGULAR, Materials.SHALE).get_rock_number(),
                              Rock(Colors.GREY, Shapes.HOOKED, Materials.SHALE).get_rock_number(),
                              Rock(Colors.GREY, Shapes.SQUARE, Materials.SHALE).get_rock_number()]
    first_gamestate.player1_rocks = ','.join([str(i) for i in player1_rocks_int_list])
    first_gamestate.player2_rocks = ','.join([str(i) for i in player2_rocks_int_list])

    first_gamestate.player1_rocks_x_coords = ','.join(["-1"] * 8)
    first_gamestate.player1_rocks_y_coords = ','.join(["-1"] * 8)
    first_gamestate.player2_rocks_x_coords = ','.join(["-2"] * 8)
    first_gamestate.player2_rocks_y_coords = ','.join(["-2"] * 8)

    first_gamestate.save()
    return first_gamestate


def validate_move(gamestate_acted_on, actor_user, source_rock_index, target_x, target_y):
    if (actor_user == gamestate_acted_on.player1_user) and (gamestate_acted_on.turns_taken%2 == 0):
        return False
    if (actor_user == gamestate_acted_on.player2_user) and (gamestate_acted_on.turns_taken%2 == 1):
        return False
    return True
    


def make_move(gamestate_acted_on, actor_user, source_rock_index, target_x, target_y):
    next_gamestate = Gamestate.get(pk=gamestate_acted_on.pk)
    next_gamestate.pk = None

    if (actor_user == gamestate_acted_on.player1_user):
        next_gamestate.player1_rock_x_coords = csv_index_assign(source_rock_index, target_x)
        next_gamestate.player1_rock_y_coords = csv_index_assign(source_rock_index, target_y)
    else:
        next_gamestate.player2_rock_x_coords = csv_index_assign(source_rock_index, target_x)
        next_gamestate.player2_rock_y_coords = csv_index_assign(source_rock_index, target_y)

    next_gamestate.save()
    return next_gamestate


def csv_to_int_list(csv):
    return [int(s) for s in csv.split(',')]


def csv_index_assign(csv, index, value):
    int_list = csv_to_int_list(csv)
    int_list[index] = value
    return ','.join([str(i) for i in int_list])
