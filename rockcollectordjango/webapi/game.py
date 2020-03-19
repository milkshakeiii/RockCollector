def make_game(player1, player2): #player1, player2 are django users
    return player1.username + " vs. " + player2.username
