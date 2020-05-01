from django.http import HttpResponse
from django.contrib.auth import authenticate, login
from django.contrib.auth.models import User
from webapi.models import Gamestate
from webapi.game import make_game, do_move, validate_move
from webapi.game import Locations, Colors, Shapes, Materials, Rock
from webapi.game import csv_to_int_list
import uuid


newline = "\n"


def MakeResponse(response_number, response_text):
    return HttpResponse(str(response_number) + newline + response_text)

def AuthFailed():
    return MakeResponse(0, "Authentication failed.")


#returns None if authentication fails
def one_message_authenticate(request):
    username = request.POST['username']
    password = request.POST['password']
    user = authenticate(request, username=username, password=password)
    if not User.objects.filter(username=username).exists():
        user = User.objects.create_user(username,
                                        'noemail@noemail.com',
                                        password)
    if user is not None:
        login(request, user)
    return user



#############################API###########################################
def index(request):
    user = one_message_authenticate(request)
    if user is not None:
        return MakeResponse(1, "Authenticated successfully: " + user.username)
    else:
        return AuthFailed()


users_looking_for_games = []
uuids_of_games_for_said_users = []
def find_game(request):
    user = one_message_authenticate(request)
    if user is None:
        return AuthFailed()

    if user in users_looking_for_games:
        return MakeResponse(2, "You're already in the queue. UUID of " +
                            "game when an opponent is found: " +
                            str(uuids_of_games_for_said_users[-1]))
    elif (len(users_looking_for_games) > 0):
        opponent = users_looking_for_games.pop()
        new_game_uuid = uuids_of_games_for_said_users.pop()
        first_gamestate = make_game(opponent, user, new_game_uuid)
        return MakeResponse(3, "Game created successfully. Game UUID: " +
                            str(new_game_uuid))
    else:
        users_looking_for_games.append(user)
        uuids_of_games_for_said_users.append(uuid.uuid4())
        return MakeResponse(4, "Added to queue.  UUID of game when an " +
                            "opponent is found: " +
                            str(uuids_of_games_for_said_users[-1]))


def check_for_gamestate(request):
    user = one_message_authenticate(request)
    if user is None:
        return AuthFailed()
    else:
        game_uuid = request.POST['game_uuid']
        turns_checking_for = int(request.POST['turns_taken'])
        try:
            gamestate_found = Gamestate.objects.get(game_uuid=game_uuid,
                                                       turns_taken=turns_checking_for)
        except Gamestate.DoesNotExist:
            return MakeResponse(5, "There is no such gamestate with " +
                               str(turns_checking_for) + " turns taken " +
                               "or there is no game with that uuid.")
        response = textify_gamestate(gamestate_found)
        return MakeResponse(6, response)
                                

def make_move(request):
    user = one_message_authenticate(request)
    if user is None:
        return AuthFailed()
    else:
        game_uuid = request.POST['game_uuid']
        turn_trying_to_take = int(request.POST['taking_turn_number'])
        source_rock_index = int(request.POST['source_rock_index'])
        target_x = int(request.POST['target_x'])
        target_y = int(request.POST['target_y'])

        try:
            gamestate_acted_on = Gamestate.objects.get(game_uuid=game_uuid,
                                                       turns_taken=turn_trying_to_take-1)
        except Gamestate.DoesNotExist:
            return MakeResponse(7, "Trying to take turn " + str(turn_trying_to_take) +
                               " but there is no gamestate with " +
                               str(turn_trying_to_take-1) + " turns taken " +
                               "or there is no game with that uuid.")
        if gamestate_acted_on.valid_action_submitted:
            return MakeResponse(8, "A valid action has already been submitted for " +
                               "turn " + str(turn_trying_to_take))

        validate_move_result = validate_move(gamestate_acted_on,
                                             user,
                                             source_rock_index,
                                             target_x,
                                             target_y)
        if (validate_move_result == "OK"):
            gamestate_acted_on.valid_action_submitted = True
            gamestate_acted_on.save()
            next_gamestate = do_move(gamestate_acted_on,
                                     user,
                                     source_rock_index,
                                     target_x,
                                     target_y)
            response = textify_gamestate(next_gamestate)
            return MakeResponse(9, response)
        else:
            return MakeResponse(10, validate_move_result + " Try again.")
##########################################################################


#viewy functions
def textify_gamestate(gamestate):
    #game id
    return_string = "Game ID: " + str(gamestate.game_uuid) + newline
    return_string += (gamestate.player1_user.username +
                      " vs. " +
                      gamestate.player2_user.username +
                      " in " +
                      Locations(gamestate.location).name +
                      newline)
    return_string += ("Turns taken so far: " +
                      str(gamestate.turns_taken) +
                      newline)
                     
    return_string += "Board:" + newline
    for i in range(gamestate.board_height):
        board_line = ''.join(
            [str(tile) for
             tile in
             csv_to_int_list(gamestate.board_tiles)[
                 (i*gamestate.board_width):((i+1)*gamestate.board_width)]]
        )
        return_string += board_line + newline

    #game status
    return_string += "Game status: "
    if (gamestate.game_over) and (gamestate.player1_wins):
        return_string += "Player 1 wins!"
    elif (gamestate.game_over):
        return_string += "Player 2 wins!"
    else:
        return_string += "Ongoing"
    return_string += newline

    #prizes
    return_string += "Prizes: "
    if len(gamestate.rocks_awarded) == 0:
        return_string += "None yet"
    for rock_number in gamestate.rocks_awarded:
        rock = Rock.rock_from_rock_number(rock_number)
        return_string += rock.get_name_string + ", "
    return_string += newline

    def coord_to_position_string(x, y):
        if x == -1 or x == -2:
            return "(ready zone)"
        if x == -3 or x == -4:
            return "(captured)"
        if x == -5:
            return "(destroyed)"
        return "(" + str(x) + ", " + str(y) + ")"
        
    for player_number in [1, 2]:
        return_string += "Player " + str(player_number) + " rocks:" + newline
        if (player_number == 1):
            rocklist = csv_to_int_list(gamestate.player1_rocks)
            rock_square_counts = csv_to_int_list(gamestate.player1_square_count_per_rock)
            x_coords = csv_to_int_list(gamestate.player1_rocks_x_coords)
            y_coords = csv_to_int_list(gamestate.player1_rocks_y_coords)
        else:
            rocklist = csv_to_int_list(gamestate.player2_rocks)
            rock_square_counts = csv_to_int_list(gamestate.player2_square_count_per_rock)
            x_coords = csv_to_int_list(gamestate.player2_rocks_x_coords)
            y_coords = csv_to_int_list(gamestate.player2_rocks_y_coords)
        coords_displayed = 0
        for i in range(0, 8):
            rock = Rock.rock_from_rock_number(rocklist[i])
            return_string += str(i) + ": " + rock.get_name_string() + " "
            for j in range(0, rock_square_counts[i]):
                return_string += coord_to_position_string(x_coords[coords_displayed],
                                                          y_coords[coords_displayed])
                coords_displayed += 1
                return_string += ", "
            return_string = return_string[0:-2]
            return_string += newline

    return_string += "Turn History: " + gamestate.turn_history
        
    return return_string
