from django.http import HttpResponse
from django.contrib.auth import authenticate, login
from django.contrib.auth.models import User
from webapi.models import Gamestate
from webapi.game import make_game, do_move, validate_move
from webapi.game import Locations, Colors, Shapes, Materials, Rock
from webapi.game import csv_to_int_list



AUTH_FAILED_MESSAGE = "Authentication failed."
newline = "<br>"


#returns None if authentication fails
def one_message_authenticate(request):
    username = request.GET['username']
    password = request.GET['password']
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
        return HttpResponse("Authenticated successfully: " + user.username)
    else:
        return HttpResponse(AUTH_FAILED_MESSAGE)


users_looking_for_games = []
def find_game(request):
    user = one_message_authenticate(request)
    if user is None:
        return HttpResponse(AUTH_FAILED_MESSAGE)

    if user in users_looking_for_games:
        return HttpResponse("You're already in the queue.")
    elif (len(users_looking_for_games) > 0):
        opponent = users_looking_for_games.pop()
        first_gamestate = make_game(user, opponent)
        response = textify_gamestate(first_gamestate)
        return HttpResponse(response)
    else:
        users_looking_for_games.append(user)
        return HttpResponse("Added to queue")


def make_move(request):
    user = one_message_authenticate(request)
    if user is None:
        return HttpResponse(AUTH_FAILED_MESSAGE)
    else:
        game_uuid = request.GET['game_uuid']
        turn_trying_to_take = int(request.GET['taking_turn_number'])
        source_rock_index = int(request.GET['source_rock_index'])
        target_x = int(request.GET['target_x'])
        target_y = int(request.GET['target_y'])

        try:
            gamestate_acted_on = Gamestate.objects.get(game_uuid=game_uuid,
                                                       turns_taken=turn_trying_to_take-1)
        except User.DoesNotExist:
            return HttpResponse("Trying to take turn " + str(turn_trying_to_take) +
                               " but there is no gamestate with " +
                               str(turn_trying_to_take-1) + " turns taken " +
                               "or there is no game with that uuid.")
        if gamestate_acted_on.valid_action_submitted:
            return HttpResponse("A valid action has already been submitted for " +
                               "turn " + str(turn_trying_to_take))

        if (validate_move(gamestate_acted_on,
                          user,
                          source_rock_index,
                          target_x,
                          target_y)):
            gamestate_acted_on.valid_action_submitted = True
            gamestate_acted_on.save()
            next_gamestate = do_move(gamestate_acted_on,
                                     user,
                                     source_rock_index,
                                     target_x,
                                     target_y)
            response = textify_gamestate(next_gamestate)
            return HttpResponse(response)
        else:
            return HttpResponse("Invalid move. Try again.")
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
             gamestate.board_tiles[
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
    return_string += newline + newline

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
            x_coords = csv_to_int_list(gamestate.player1_rocks_x_coords)
            y_coords = csv_to_int_list(gamestate.player1_rocks_y_coords)
        else:
            rocklist = csv_to_int_list(gamestate.player2_rocks)
            x_coords = csv_to_int_list(gamestate.player2_rocks_x_coords)
            y_coords = csv_to_int_list(gamestate.player2_rocks_y_coords)
        for i in range(0, 8):
            rock = Rock.rock_from_rock_number(rocklist[i])
            return_string += str(i) + ": " + rock.get_name_string() + " "
            return_string += coord_to_position_string(
                x_coords[i],
                y_coords[i])
            return_string += newline
        
    return return_string
