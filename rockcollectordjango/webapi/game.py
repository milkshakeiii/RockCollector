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
    RED = 3
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
                              Rock(Colors.RED, Shapes.HOOKED, Materials.SHALE).get_rock_number(),
                              Rock(Colors.BLUE, Shapes.TRIANGULAR, Materials.SHALE).get_rock_number(),
                              Rock(Colors.RED, Shapes.STARSHAPED, Materials.SHALE).get_rock_number(),
                              Rock(Colors.BLUE, Shapes.CIRCULAR, Materials.SHALE).get_rock_number(),
                              Rock(Colors.GREY, Shapes.TRIANGULAR, Materials.SHALE).get_rock_number(),
                              Rock(Colors.RED, Shapes.HOOKED, Materials.SHALE).get_rock_number(),
                              Rock(Colors.BLUE, Shapes.SQUARE, Materials.SHALE).get_rock_number()]
    player2_rocks_int_list = [Rock(Colors.BLUE, Shapes.SQUARE, Materials.SHALE).get_rock_number(),
                              Rock(Colors.GREY, Shapes.HOOKED, Materials.SHALE).get_rock_number(),
                              Rock(Colors.RED, Shapes.TRIANGULAR, Materials.SHALE).get_rock_number(),
                              Rock(Colors.BLUE, Shapes.STARSHAPED, Materials.SHALE).get_rock_number(),
                              Rock(Colors.GREY, Shapes.CIRCULAR, Materials.SHALE).get_rock_number(),
                              Rock(Colors.RED, Shapes.TRIANGULAR, Materials.SHALE).get_rock_number(),
                              Rock(Colors.RED, Shapes.HOOKED, Materials.SHALE).get_rock_number(),
                              Rock(Colors.BLUE, Shapes.SQUARE, Materials.SHALE).get_rock_number()]
    first_gamestate.player1_rocks = ','.join([str(i) for i in player1_rocks_int_list])
    first_gamestate.player2_rocks = ','.join([str(i) for i in player2_rocks_int_list])

    first_gamestate.player1_square_count_per_rock = ','.join(["1"] * 8)
    first_gamestate.player1_rocks_x_coords = ','.join(["-1"] * 8)
    first_gamestate.player1_rocks_y_coords = ','.join(["-1"] * 8)
    first_gamestate.player2_square_count_per_rock = ','.join(["1"] * 8)
    first_gamestate.player2_rocks_x_coords = ','.join(["-2"] * 8)
    first_gamestate.player2_rocks_y_coords = ','.join(["-2"] * 8)

    first_gamestate.save()
    return first_gamestate

#no side effects
def add_coords(a, b):
    return (a[0] + b[0], a[1] + b[1])

#no side effects
#returns a list of lists, where each list is a line
#example, a king or queen would have 8 lines,
#whereas a rook or bishop would have 4
def get_move_lines(start_square, shape, width, height):
    up = [add_coords(start_square, (0, i)) for i in range(1, width)]
    down = [add_coords(start_square, (0, -i)) for i in range(1, width)]
    right = [add_coords(start_square, (i, 0)) for i in range(1, width)]
    left = [add_coords(start_square, (-i, 0)) for i in range(1, width)]
    upright = [add_coords(start_square, (i, i)) for i in range(1, width)]
    upleft = [add_coords(start_square, (i, -i)) for i in range(1, width)]
    downright = [add_coords(start_square, (-i, i)) for i in range(1, width)]
    downleft = [add_coords(start_square, (-i, -i)) for i in range(1, width)]

    ##################################################################################
    if shape == Shapes.CIRCULAR:
        move_lines = [add_coords(start_square, (1, -1)),
                      add_coords(start_square, (1, 0)),
                      add_coords(start_square, (1, 1)),
                      add_coords(start_square, (0, -1)),
                      add_coords(start_square, (0, 1)),
                      add_coords(start_square, (-1, -1)),
                      add_coords(start_square, (-1, 0)),
                      add_coords(start_square, (-1, 1))]
    if shape == Shapes.SQUARE:
        move_lines = [up, down, left, right]
    if shape == Shapes.TRIANGULAR:
        move_lines = [upright, upleft, downright, downleft]
    if shape == Shapes.STARSHAPED:
        move_lines = [upright, upleft, downright, downleft, up, right, down, left]
    if shape == Shapes.LUMPY:
        move_lines = [[add_coords(start_square, (0, 1))], [add_coords(shape, (0, -1))]]
    if shape == Shapes.HOOKED:
        move_lines = [[add_coords(start_square, (2, 1))],
                      [add_coords(start_square, (2, -1))],
                      [add_coords(start_square, (-2, 1))],
                      [add_coords(start_square, (-2, -1))],
                      [add_coords(start_square, (1, 2))],
                      [add_coords(start_square, (1, -2))],
                      [add_coords(start_square, (-1, 2))],
                      [add_coords(start_square, (-1, -2))]]
    ##################################################################################

    for i in range(len(move_lines)):
        line = move_lines[i]
        move_lines[i] = [move for move in line if in_bounds(move, width, height)]

    return move_lines
        

#no side effects (obv)
def in_bounds(square, width, height):
    return square[0] >= 0 and square[0] < width and square[1] >= 0 and square[1] < height

#no side effects
#takes a source_rock that is on the board
#returns a list of valid target squares for action originating from that source
def get_onboard_targetable_squares(source_rock_index,
                                   source_rock,
                                   source_rock_squares,
                                   active_player_rocks,
                                   active_player_rock_squares,
                                   all_active_player_occupied_squares, #list of (x, y) squares
                                   other_player_rocks,
                                   other_player_rock_squares,
                                   all_other_player_occupied_squares, #list of (x, y) squares
                                   board_rows,
                                   width,
                                   height):

    if len(source_rock_squares) == 0:
        raise "get_onboard_targetable_squares got no start squares"
    in_ready_zone = (-1, -1) in source_rock_squares or (-2, -2) in source_rock_squares
    in_captured_zone = (-3, -3) in source_rock_squares or (-4, -4) in source_rock_squares
    in_destroyed_zone = (-5, -5) in source_rock_squares
    if in_ready_zone or in_captured_zone or in_destroyed_zone:
        raise "get_onboard_targetable_squares got start squares not on board: " + str(start_squares)
    
    start_square = source_rock_squares[0]

    ##################################################################################
    if (source_rock.color == Colors.GREY):
        targetable_squares = []
        move_lines = get_move_lines(start_square, source_rock.shape, width, height)
        for line in move_lines:
            for square in line:
                if not in_bounds(square, width, height):
                    break
                if square in all_active_player_occupied_squares:
                    break
                elif square in all_other_player_occupied_squares:
                    targetable_squares.append(square)
                    break
                else:
                    targetable_squares.append(square)
        return targetable_squares
    if (source_rock.color == Colors.BLUE):
        return source_rock_squares
    if (source_rock.color == Colors.RED):
        targetable_squares = []
        move_lines = get_move_lines(start_square, source_rock.shape, width, height)
        for line in move_lines:
            for square in line:
                if not in_bounds(square, width, height):
                    break
                else:
                    targetable_squares.append(square)
        return targetable_squares
    ##################################################################################

    raise Exception(str(source_rock.shape) + ", " + str(source_rock.color) + " didn't match a targeting scheme")
        

#no side effects
#returns: list of (x, y) squares that are valid targets for action originating from source_rock
def get_valid_target_squares(source_rock_index,
                             source_rock,
                             source_rock_squares,
                             active_player_rocks,
                             active_player_rock_squares,
                             other_player_rocks,
                             other_player_rock_squares,
                             board_rows,
                             width,
                             height):
    if (len(board_rows) == 0):
        raise Exception("Empty board rows")

    all_active_player_occupied_squares = []
    for i in range(0, len(active_player_rocks)):
        rock = active_player_rocks[i]
        squares = active_player_rock_squares[i]
        if not (rock.color == Colors.BLUE and len(squares) > 1):
            all_active_player_occupied_squares.extend(squares)
    all_other_player_occupied_squares = []
    for i in range(0, len(other_player_rocks)):
        rock = other_player_rocks[i]
        squares = other_player_rock_squares[i]
        if not (rock.color == Colors.BLUE and len(squares) > 1):
            all_other_player_occupied_squares.extend(squares)

    valid_target_squares = []
    #place from ready zone: recall (-1, -1) is player 1's ready zone, (-2, -2) is player 2's
    if (source_rock_squares == [(-1, -1)] or source_rock_squares == [(-2, -2)]):
        for x in range(width):
            for y in range(height):
                actor_rock_overlap = (x, y) in all_active_player_occupied_squares
                other_rock_overlap = (x, y) in all_other_player_occupied_squares
                if not actor_rock_overlap and not other_rock_overlap:
                    valid_target_squares.append((x, y))
    else:
        valid_target_squares = get_onboard_targetable_squares(source_rock_index,
                                                              source_rock,
                                                              source_rock_squares,
                                                              active_player_rocks,
                                                              active_player_rock_squares,
                                                              all_active_player_occupied_squares,
                                                              other_player_rocks,
                                                              other_player_rock_squares,
                                                              all_other_player_occupied_squares,
                                                              board_rows,
                                                              width,
                                                              height)
    return valid_target_squares
    
#no side effects
#returns list of rows
#rows are lists of BoardTiles enum instances
def get_board_rows_from_csv(csv_board, height, width):
    rows = []
    for i in range(height):
        row = [BoardTiles(tile) for
               tile in
               csv_to_int_list(csv_board)[(i*width):((i+1)*width)]]
        rows.append(row)
    return rows

#no side effects
#returns dict where keys are rock index, values are list of all (x, y) squares occupied by that rock
def get_square_dict(squares_per_rock, rock_x_coords, rock_y_coords):
    square_dict = {}
    for i in range(len(squares_per_rock)):
        count = squares_per_rock[i]
        square_dict[i] = [(rock_x_coords.pop(0), rock_y_coords.pop(0)) for j in range(count)]
    return square_dict

#no side effects
#returns "OK" if valid move, otherwise string explaining why it's invalid
def validate_move(gamestate_acted_on, actor_user, source_rock_index, target_x, target_y):
    #invalid if the wrong player is trying to act
    player1_active = (actor_user == gamestate_acted_on.player1_user)
    if player1_active and (gamestate_acted_on.turns_taken%2 == 1):
        return "Wrong player submitting turn."
    if not player1_active and (gamestate_acted_on.turns_taken%2 == 0):
        return "Wrong player submitting turn."

    board_width = gamestate_acted_on.board_width
    board_height = gamestate_acted_on.board_height
    board_rows = get_board_rows_from_csv(gamestate_acted_on.board_tiles, board_height, board_width)

    (active_player_rocks,
     other_player_rocks,
     active_player_rock_squares,
     other_player_rock_squares) = get_rock_data_from_gamestate(gamestate_acted_on, player1_active)

    source_rock = active_player_rocks[source_rock_index]
    source_rock_squares = active_player_rock_squares[source_rock_index]
    valid_target_squares = get_valid_target_squares(source_rock_index,
                                                    source_rock,
                                                    source_rock_squares,
                                                    active_player_rocks,
                                                    active_player_rock_squares,
                                                    other_player_rocks,
                                                    other_player_rock_squares,
                                                    board_rows,
                                                    board_width,
                                                    board_height)

    target_square = (target_x, target_y)
    if target_square not in valid_target_squares:
        if len(valid_target_squares) == 0:
            return "That's not a valid target square. It looks like that rock can't move."
        else:
            return "That's not a valid target square. Valid squares are: " + str(valid_target_squares)                               

    return "OK"
    
#extracts data, does move based on rock color in marked section
#calls apply_rock_data_to_gamestate, then  saves
#yes side effects (TO THE DATABASE, creates a NEW gamestate
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

    player1_active = (actor_user == gamestate_acted_on.player1_user)
        
    (active_player_rocks,
     other_player_rocks,
     active_player_rock_squares,
     other_player_rock_squares) = get_rock_data_from_gamestate(gamestate_acted_on, player1_active)

    source_rock = active_player_rocks[source_rock_index]
    source_rock_root_square = active_player_rock_squares[source_rock_index][0]
    width = next_gamestate.board_width
    height = next_gamestate.board_height
    
    ##################################################################################
    if (source_rock_root_square in [(-1, -1), (-2, -2)]): #so far, placement from the ready zone is just
        active_player_rock_squares[source_rock_index] = [(target_x, target_y)] #this
    elif source_rock.color == Colors.GREY:
        if (has_occupant((target_x, target_y), other_player_rock_squares)):
            occupant_index = get_occupant((target_x, target_y), other_player_rock_squares)
            occupant_is_blue = other_player_rocks[occupant_index].color == Colors.BLUE
            occupant_is_multisquared = len(other_player_rock_squares[occupant_index]) > 1
            if not (occupant_is_blue and occupant_is_multisquared):
                other_player_rock_squares[occupant_index] = [(-3, -3)] if player1_active else [(-4, -4)]
        active_player_rock_squares[source_rock_index] = [(target_x, target_y)]
    elif source_rock.color == Colors.BLUE:
        source_rock_squares = active_player_rock_squares[source_rock_index]
        if len(source_rock_squares) == 1:
            for line in get_move_lines(source_rock_squares[0],
                                       source_rock.shape,
                                       width,
                                       height):
                source_rock_squares.extend(line)
        else:
            active_player_rock_squares[source_rock_index] = [(target_x, target_y)]
    elif source_rock.color == Colors.RED:
        source_rock_squares = active_player_rock_squares[source_rock_index]
        for line in get_move_lines(source_rock_squares[0],
                                   source_rock.shape,
                                   width,
                                   height):
            for square in line:
                if (has_occupant(square, other_player_rock_squares)):
                    enemy_occupant_index = get_occupant(square, other_player_rock_squares)
                    other_player_rock_squares[enemy_occupant_index] = [(-5, -5)]
                if (has_occupant(square, active_player_rock_squares)):
                    actor_occupant_index = get_occupant(square, active_player_rock_squares)
                    active_player_rock_squares[actor_occupant_index] = [(-5, -5)]
    ##################################################################################

    
    apply_rock_data_to_gamestate(player1_active,
                                 next_gamestate,
                                 active_player_rocks,
                                 other_player_rocks,
                                 active_player_rock_squares,
                                 other_player_rock_squares)
    next_gamestate.save()
    return next_gamestate

#no side effects
def get_rock_data_from_gamestate(gamestate_acted_on, player1_active):
    if player1_active:
        active_player_squares_per_rock = csv_to_int_list(gamestate_acted_on.player1_square_count_per_rock)
        active_player_rock_number_list = csv_to_int_list(gamestate_acted_on.player1_rocks)
        active_player_rock_x_coords = csv_to_int_list(gamestate_acted_on.player1_rocks_x_coords)
        active_player_rock_y_coords = csv_to_int_list(gamestate_acted_on.player1_rocks_y_coords)
        other_player_squares_per_rock = csv_to_int_list(gamestate_acted_on.player2_square_count_per_rock)
        other_player_rock_number_list = csv_to_int_list(gamestate_acted_on.player2_rocks)
        other_player_rock_x_coords = csv_to_int_list(gamestate_acted_on.player2_rocks_x_coords)
        other_player_rock_y_coords = csv_to_int_list(gamestate_acted_on.player2_rocks_y_coords)
    else:
        other_player_rock_number_list = csv_to_int_list(gamestate_acted_on.player1_rocks)
        other_player_rock_x_coords = csv_to_int_list(gamestate_acted_on.player1_rocks_x_coords)
        other_player_rock_y_coords = csv_to_int_list(gamestate_acted_on.player1_rocks_y_coords)
        other_player_squares_per_rock = csv_to_int_list(gamestate_acted_on.player1_square_count_per_rock)
        active_player_rock_number_list = csv_to_int_list(gamestate_acted_on.player2_rocks)
        active_player_rock_x_coords = csv_to_int_list(gamestate_acted_on.player2_rocks_x_coords)
        active_player_rock_y_coords = csv_to_int_list(gamestate_acted_on.player2_rocks_y_coords)
        active_player_squares_per_rock = csv_to_int_list(gamestate_acted_on.player2_square_count_per_rock)
        
    active_player_rocks = [Rock.rock_from_rock_number(rock_number) for
                           rock_number in active_player_rock_number_list]
    other_player_rocks =  [Rock.rock_from_rock_number(rock_number) for
                           rock_number in other_player_rock_number_list]

    active_player_rock_squares = get_square_dict(active_player_squares_per_rock,
                                                 active_player_rock_x_coords,
                                                 active_player_rock_y_coords)
    other_player_rock_squares = get_square_dict(other_player_squares_per_rock,
                                                other_player_rock_x_coords,
                                                other_player_rock_y_coords)

    return (active_player_rocks,
            other_player_rocks,
            active_player_rock_squares,
            other_player_rock_squares)

#yes side effects (to gamestate)
def apply_rock_data_to_gamestate(player1_active,
                                 gamestate,
                                 active_player_rocks,
                                 other_player_rocks,
                                 active_player_rock_squares,
                                 other_player_rock_squares):
    active_player_square_count_per_rock = []
    active_player_rock_squares_list = []
    for i in range(len(active_player_rocks)):
        squares = active_player_rock_squares[i]
        active_player_square_count_per_rock.append(str(len(squares)))
        active_player_rock_squares_list.extend(squares)
    active_player_square_count_per_rock_csv = ','.join(active_player_square_count_per_rock)
    active_player_rocks_x_coords_csv = ','.join([str(square[0]) for
                                                 square in active_player_rock_squares_list])
    active_player_rocks_y_coords_csv = ','.join([str(square[1]) for
                                                 square in active_player_rock_squares_list])

    other_player_square_count_per_rock = []
    other_player_rock_squares_list = []
    for i in range(len(other_player_rocks)):
        squares = other_player_rock_squares[i]
        other_player_square_count_per_rock.append(str(len(squares)))
        other_player_rock_squares_list.extend(squares)
    other_player_square_count_per_rock_csv = ','.join(other_player_square_count_per_rock)
    other_player_rocks_x_coords_csv = ','.join([str(square[0]) for
                                                square in other_player_rock_squares_list])
    other_player_rocks_y_coords_csv = ','.join([str(square[1]) for
                                                square in other_player_rock_squares_list])

    if (player1_active):
        gamestate.player1_square_count_per_rock = active_player_square_count_per_rock_csv
        gamestate.player1_rocks_x_coords = active_player_rocks_x_coords_csv
        gamestate.player1_rocks_y_coords = active_player_rocks_y_coords_csv
        gamestate.player2_square_count_per_rock = other_player_square_count_per_rock_csv
        gamestate.player2_rocks_x_coords = other_player_rocks_x_coords_csv
        gamestate.player2_rocks_y_coords = other_player_rocks_y_coords_csv
    else:
        gamestate.player2_square_count_per_rock = active_player_square_count_per_rock_csv
        gamestate.player2_rocks_x_coords = active_player_rocks_x_coords_csv
        gamestate.player2_rocks_y_coords = active_player_rocks_y_coords_csv
        gamestate.player1_square_count_per_rock = other_player_square_count_per_rock_csv
        gamestate.player1_rocks_x_coords = other_player_rocks_x_coords_csv
        gamestate.player1_rocks_y_coords = other_player_rocks_y_coords_csv

#no side effects
def has_occupant(square, rock_squares):
    for key, value in rock_squares.items():
        if square in value:
            return True
    return False

#no side effects
def get_occupant(square, rock_squares):
    for key, value in rock_squares.items():
        if square in value:
            return key
    return None

#no side effects
def csv_to_int_list(csv):
    split_csv = csv.split(',')
    if ('' in split_csv):
        raise Exception(str(split_csv))
    return [int(s) for s in split_csv]

#no side effects
def csv_index_assign(csv, index, value):
    int_list = csv_to_int_list(csv)
    int_list[index] = value
    return ','.join([str(i) for i in int_list])
