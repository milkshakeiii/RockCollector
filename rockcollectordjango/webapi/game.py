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
    first_gamestate.game_uuid = uuid.uuid4()
    first_gamestate.board_width = 8
    first_gamestate.board_height = 8
    first_gamestate.location = Locations.JUNGLE.value

    first_gamestate.board_tiles = (','.join([str(BoardTiles.PLAIN_GROUND.value)] *
                                             first_gamestate.board_width *
                                             first_gamestate.board_height))
    
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


def get_valid_target_squares(source_rock,
                             source_rock_square,
                             active_player_rocks,
                             active_player_rock_squares,
                             other_player_rocks,
                             other_player_rock_squares,
                             board_rows,
                             width,
                             height):
    if (len(board_rows) == 0):
        raise Exception("Empty board rows")
    valid_squares = []

    for x in range(width):
        for y in range(height):
            player_rock_overlap = (x, y) in active_player_rock_squares
            other_rock_overlap = (source_rock_square == (-1, -1) and (x, y) in other_player_rock_squares)
            if not player_rock_overlap and not other_rock_overlap:
                valid_squares.append((x, y))

    return valid_squares
    


def get_board_rows_from_csv(csv_board, height, width):
    rows = []
    for i in range(height):
        row = [BoardTiles(tile) for
               tile in
               csv_to_int_list(csv_board)[(i*width):((i+1)*width)]]
        rows.append(row)
    return rows


def validate_move(gamestate_acted_on, actor_user, source_rock_index, target_x, target_y):
    #invalid if the wrong player is trying to act
    player1_active = (actor_user == gamestate_acted_on.player1_user)
    if player1_active and (gamestate_acted_on.turns_taken%2 == 0):
        return False
    if not player1_active and (gamestate_acted_on.turns_taken%2 == 1):
        return False

    board_width = gamestate_acted_on.board_width
    board_height = gamestate_acted_on.board_height
    board_rows = get_board_rows_from_csv(gamestate_acted_on.board_tiles, board_height, board_width)

    if player1_active:
        active_player_rock_number_list = csv_to_int_list(gamestate_acted_on.player1_rocks)
        active_player_rock_x_coords = csv_to_int_list(gamestate_acted_on.player1_rocks_x_coords)
        active_player_rock_y_coords = csv_to_int_list(gamestate_acted_on.player1_rocks_y_coords)
        other_player_rock_number_list = csv_to_int_list(gamestate_acted_on.player2_rocks)
        other_player_rock_x_coords = csv_to_int_list(gamestate_acted_on.player2_rocks_x_coords)
        other_player_rock_y_coords = csv_to_int_list(gamestate_acted_on.player2_rocks_y_coords)
    else:
        other_player_rock_number_list = csv_to_int_list(gamestate_acted_on.player1_rocks)
        other_player_rock_x_coords = csv_to_int_list(gamestate_acted_on.player1_rocks_x_coords)
        other_player_rock_y_coords = csv_to_int_list(gamestate_acted_on.player1_rocks_y_coords)
        active_player_rock_number_list = csv_to_int_list(gamestate_acted_on.player2_rocks)
        active_player_rock_x_coords = csv_to_int_list(gamestate_acted_on.player2_rocks_x_coords)
        active_player_rock_y_coords = csv_to_int_list(gamestate_acted_on.player2_rocks_y_coords)
        
    source_rock = Rock.rock_from_rock_number(active_player_rock_number_list[source_rock_index])
    source_rock_x = active_player_rock_x_coords[source_rock_index]
    source_rock_y = active_player_rock_y_coords[source_rock_index]
    source_rock_square = (source_rock_x, source_rock_y)
    active_player_rocks = [Rock.rock_from_rock_number(rock_number) for
                           rock_number in active_player_rock_number_list]
    other_player_rocks =  [Rock.rock_from_rock_number(rock_number) for
                           rock_number in other_player_rock_number_list]
    active_player_rock_squares = [(active_player_rock_x_coords[i], active_player_rock_y_coords[i]) for
                                  i in range(0, len(active_player_rock_number_list))]
    other_player_rock_squares = [(other_player_rock_x_coords[i], other_player_rock_y_coords[i]) for
                                 i in range(0, len(other_player_rock_number_list))]

    valid_target_squares = get_valid_target_squares(source_rock,
                                                    source_rock_square,
                                                    active_player_rocks,
                                                    active_player_rock_squares,
                                                    other_player_rocks,
                                                    other_player_rock_squares,
                                                    board_rows,
                                                    board_width,
                                                    board_height)

    target_square = (target_x, target_y)
    if target_square not in valid_target_squares:
        return False                                

    return True
    


def do_move(gamestate_acted_on, actor_user, source_rock_index, target_x, target_y):
    next_gamestate = Gamestate.objects.get(pk=gamestate_acted_on.pk)
    next_gamestate.pk = None
    next_gamestate.turns_taken += 1
    next_gamestate.valid_action_submitted = False
    if next_gamestate.turn_history == "":
        next_gamestate.turn_history = ','.join([str(source_rock_index),
                                                str(target_x),
                                                str(target_y)])
    else:
        next_gamestate.turn_history = ','.join([next_gamestate.turn_history,
                                                str(source_rock_index),
                                                str(target_x),
                                                str(target_y)])
    
    if (actor_user == gamestate_acted_on.player1_user):
        next_gamestate.player1_rocks_x_coords = csv_index_assign(next_gamestate.player1_rocks_x_coords,
                                                                 source_rock_index,
                                                                 target_x)
        next_gamestate.player1_rocks_y_coords = csv_index_assign(next_gamestate.player1_rocks_x_coords,
                                                                 source_rock_index,
                                                                 target_y)
    else:
        next_gamestate.player2_rocks_x_coords = csv_index_assign(next_gamestate.player2_rocks_x_coords,
                                                                 source_rock_index,
                                                                 target_x)
        next_gamestate.player2_rocks_y_coords = csv_index_assign(next_gamestate.player2_rocks_x_coords,
                                                                 source_rock_index,
                                                                 target_y)

    next_gamestate.save()
    return next_gamestate


def csv_to_int_list(csv):
    split_csv = csv.split(',')
    if ('' in split_csv):
        raise Exception(str(split_csv))
    return [int(s) for s in split_csv]


def csv_index_assign(csv, index, value):
    int_list = csv_to_int_list(csv)
    int_list[index] = value
    return ','.join([str(i) for i in int_list])
