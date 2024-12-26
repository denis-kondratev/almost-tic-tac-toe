# Almost Tic-Tac-Toe with Unity and ML Agents

Implementation of the **Almost Tic-Tac-Toe** board game in Unity using
ML-Agents for training an AI to play the game.

![tic-tac-toe.gif](tic-tac-toe.gif)

## Game Rules

The game is played on a **3x3 grid**, and the rules are similar to standard
Tic-Tac-Toe but with one important difference:  
each player has **7 pieces of different sizes**, resembling a nested doll
(matryoshka). Smaller pieces can be covered by larger ones.

On their turn, a player can:
- Place a piece on an empty cell.
- Cover their own or the opponent's piece if the size of the piece allows it.

The player who first lines up their pieces in a row (horizontally, 
vertically, or diagonally) wins, just like in the standard Tic-Tac-Toe game.