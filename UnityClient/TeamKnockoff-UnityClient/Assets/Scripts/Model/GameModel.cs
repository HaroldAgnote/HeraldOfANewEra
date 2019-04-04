using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Assets.Scripts.Model.Units;
using Assets.Scripts.Model.Skills;
using Assets.Scripts.Model.Tiles;
using Assets.Scripts.Utilities.ExtensionMethods;
using Assets.Scripts.Utilities.WeightedGraph;

namespace Assets.Scripts.Model {

    /// <summary>
    /// <c>GameModel</c> is the model representation of a Game.
    /// </summary>
    public class GameModel : MonoBehaviour {

        #region Member fields

        /// <summary>
        /// 2D Array to keep track of Units and their positions
        /// </summary>
        private Unit[,] mUnits;

        /// <summary>
        /// 2D Array to keep track of Tiles and their positions 
        /// </summary>
        private Tile[,] mTiles;

        /// <summary>
        /// List to keep track of Players in the current Game 
        /// </summary>
        private List<Player> mPlayers;

        #endregion

        #region Properties

        /// <summary>
        /// Columns in the Game Map
        /// </summary>
        public int Columns { get; private set; }

        /// <summary>
        /// Rows in the Game Map
        /// </summary>
        public int Rows { get; private set; }

        /// <summary>
        /// Current Turn in the Game
        /// </summary>
        public int Turn { get; private set; }

        /// <summary>
        /// The Current Player controlling the Game 
        /// </summary>
        public Player CurrentPlayer { get; private set; }

        /// <summary>
        /// Determines if the Current Player has any Units that can move
        /// </summary>
        public bool CurrentPlayerHasNoMoves {
            get {
                return CurrentPlayer.Units
                            .Where(unit => unit.IsAlive)
                            .All(unit => unit.HasMoved);
            }
        }


        /// <summary>
        /// Determines if the Game has ended
        /// </summary>

        public bool GameHasEnded {
            get {
                // Need a better way to check if game has ended 
                // With multiple players
                if (mPlayers.Count == 2) {
                    var playerOne = mPlayers[0];
                    var playerTwo = mPlayers[1];

                    if (!playerOne.HasAliveUnit()) {
                        return true;
                    } else if (!playerTwo.HasAliveUnit()) {
                        return true;
                    }
                    return false;
                } else {
                    return false;
                }
            }
        }


        #endregion

        #region Public methods (Initialization)

        /// <summary>
        /// Initializes the Model of the Game
        /// </summary>
        /// <param name="cols">Number of columns of the Map</param>
        /// <param name="rows">Number of rows of the Map</param>
        /// <param name="players">Players that will be in the game</param>

        public void ConstructModel(int cols, int rows, IEnumerable<Player> players) {
            // Initialize members and properties
            Columns = cols;
            Rows = rows;

            mUnits = new Unit[Columns, Rows];
            mTiles = new Tile[Columns, Rows];

            mPlayers = new List<Player>();
            mPlayers.AddRange(players);

            // Start turns
            Turn = 1;
        }

        /// <summary>
        /// Starts the Game by setting the Current Player to the first player in the 
        /// list and starting their turn 
        /// </summary>

        public void StartGame() {
            foreach (var player in mPlayers) {
                foreach (var unit in player.Units) {
                    var passiveSkills = unit.Skills
                                            .Where(skill => skill is FieldSkill)
                                            .Select(skill => skill as FieldSkill);

                    foreach (var skill in passiveSkills) {
                        skill.ApplyFieldSkill(unit);
                    }
                }
            }
            CurrentPlayer = mPlayers.First();
            CurrentPlayer.StartTurn();
        }

        /// <summary>
        /// Adds a Unit to the Game
        /// </summary>
        /// <param name="unit">Unit to be added to the game</param>
        /// <param name="playerNum">index number of the Player that will own the Unit</param>
        /// <param name="col">Column for Unit to spawn in</param>
        /// <param name="row">Row for Unit to spawn in</param>

        public void AddUnit(Unit unit, int playerNum, int col, int row) {
            mPlayers[playerNum - 1].AddUnit(unit);
            mUnits[col, row] = unit;
        }

        /// <summary>
        /// Adds a Unit to the Game
        /// </summary>
        /// <param name="unit">Unit to be added to the game</param>
        /// <param name="player">Player that will own the Unit</param>
        /// <param name="col">Column for Unit to spawn in</param>
        /// <param name="row">Row for Unit to spawn in</param>

        public void AddUnit(Unit unit, Player player, int col, int row) {
            player.AddUnit(unit);
            mUnits[col, row] = unit;
        }

        /// <summary>
        /// Adds a tile to the game map
        /// </summary>
        /// <param name="tile">Tile to be added to the Game map</param>

        public void AddTile(Tile tile) {
            mTiles[tile.Position.x, tile.Position.y] = tile;
        }

        /// <summary>
        /// Goes through each tile in the Game Map and adds neighbors
        /// </summary>
        public void AddNeighbors() {
            for (int column = 0; column < Columns; column++) {
                for (int row = 0; row < Rows; row++) {
                    var tile = mTiles[column, row];
                    try {
                        mTiles[column - 1, row].Neighbors.Add(tile);
                    } catch { }
                    try {
                        mTiles[column + 1, row].Neighbors.Add(tile);
                    } catch { }

                    try {
                        mTiles[column, row - 1].Neighbors.Add(tile);
                    } catch { }
                    try {
                        mTiles[column, row + 1].Neighbors.Add(tile);
                    } catch { }
                }
            }
        }


        #endregion

        #region Public Methods

        /// <summary>
        /// Retrieves a Unit at a certain position
        /// </summary>
        /// <param name="vector">(x, y) position to get the Unit</param>
        /// <returns>
        /// Returns the Unit at the given position.
        /// Returns null if no Unit exists at that position.
        /// </returns>

        public Unit GetUnitAtPosition(Vector2Int vector) {
            return GetUnitAtPosition(vector.x, vector.y);
        }

        /// <summary>
        /// Retrieves a Unit at a certain position
        /// </summary>
        /// <param name="col">Column (X Position) of Unit</param>
        /// <param name="row">Row (Y position) of Unit</param>
        /// <returns>
        /// Returns the Unit at the given position.
        /// Returns null if no Unit exists at that position.
        /// </returns>

        public Unit GetUnitAtPosition(int col, int row) {
            try {
                return mUnits[col, row];
            } catch {
                return null;
            }
        }

        /// <summary>
        /// Retrieves a Tile at a certain position
        /// </summary>
        /// <param name="vector">(x, y) position to get the Tile </param>
        /// <returns>
        /// Returns the Tile at the given position.
        /// Returns null if no Tile exists at that position.
        /// </returns>

        public Tile GetTileAtPosition(Vector2Int vector) {
            return GetTileAtPosition(vector.x, vector.y);
        }

        /// <summary>
        /// Retrieves a Tile at a certain position
        /// </summary>
        /// <param name="col">Column (X Position) of Tile</param>
        /// <param name="row">Row (Y position) of Tile</param>
        /// <returns>
        /// Returns the Tile at the given position.
        /// Returns null if no Tile exists at that position.
        /// </returns>
        public Tile GetTileAtPosition(int col, int row) {
            try {
                return mTiles[col, row];
            } catch {
                return null;
            }
        }

        /// <summary>
        /// Retrieves the position of a given Unit
        /// </summary>
        /// <param name="unit">Unit to find position of</param>
        /// <returns>
        /// Returns the Vector of the Unit's position
        /// Returns null if no Unit could be found
        /// </returns>
        public Vector2Int GridForUnit(Unit unit) {
            for (int col = 0; col < mUnits.GetLength(0); col++) {
                for (int row = 0; row < mUnits.GetLength(1); row++) {
                    if (mUnits[col, row] == unit) {
                        return new Vector2Int(col, row);
                    }
                }
            }

            return new Vector2Int(-1, -1);
        }

        /// <summary>
        /// Determines if a given Unit is owned by the Current Player controlling the Game.
        /// </summary>
        /// <param name="unit">Unit to determine if Current Player's owner</param>
        /// <returns>
        /// Returns the Vector of the Unit's position
        /// Returns null if no Unit could be found
        /// </returns>

        public bool DoesUnitBelongToCurrentPlayer(Unit unit) {
            return CurrentPlayer.Units.Contains(unit);
        }

        /// <summary>
        /// Determines if a Tile at a given position is occupied by another Unit.
        /// </summary>
        /// <param name="position">The position of the Tile that will be checked.</param>
        /// <returns>
        /// Returns <c>true</c> if the tile is occupied, <c>false</c> otherwise.
        /// </returns>

        public bool TileIsOccupied(Vector2Int position) {
            return GetUnitAtPosition(position) != null && GetUnitAtPosition(position).IsAlive;
        }

        /// <summary>
        /// Checks if an Enemy is within range of a position with some range
        /// </summary>
        /// <param name="position">The position to check surrounding locations</param>
        /// <param name="range">Range from the position to check</param>
        /// <returns>
        /// Returns <c>true</c> if an enemy is within range of the position, <c>false</c>
        /// otherwise.
        /// </returns>

        public bool EnemyWithinRange(Vector2Int position, int range) {
            var surroundingLocations = GetSurroundingAttackLocationsAtPoint(position, range);

            return surroundingLocations.Any(pos => EnemyAtLocation(pos));
        }

        /// <summary>
        /// Checks if an Ally is within range of a position with some range
        /// </summary>
        /// <param name="position">The position to check surrounding locations</param>
        /// <param name="range">Range from the position to check</param>
        /// <returns>
        /// Returns <c>true</c> if an Ally is within range of the position, <c>false</c>
        /// otherwise.
        /// </returns>
        
        public bool AllyWithinRange(Vector2Int position, int range) {
            var surroundingLocations = GetSurroundingAttackLocationsAtPoint(position, range);

            return surroundingLocations.Any(pos => AllyAtLocation(pos));
        }

        /// <summary>
        /// Determines if there is an Enemy of the Current Player at a given location.
        /// </summary>
        /// <param name="location">Location to check for an Enemy</param>
        /// <returns>
        /// Returns <c>true</c> if there is an enemy at the location, <c>false</c>
        /// otherwise.
        /// </returns>

        public bool EnemyAtLocation(Vector2Int location) {
            var unit = GetUnitAtPosition(location);
            return unit != null && unit.IsAlive && !CurrentPlayer.Units.Contains(unit);
        }

        /// <summary>
        /// Determines if there is an Ally of the Current Player at a given location.
        /// </summary>
        /// <param name="location">Location to check for an Ally</param>
        /// <returns>
        /// Returns <c>true</c> if there is an Ally at the location, <c>false</c>
        /// otherwise.
        /// </returns>

        public bool AllyAtLocation(Vector2Int location) {
            var unit = GetUnitAtPosition(location);
            return unit != null && unit.IsAlive && CurrentPlayer.Units.Contains(unit);
        }
        

        /// <summary>
        /// Retrieves a copy of the 2D Unit map of the Game
        /// </summary>
        /// <returns>
        /// Returns a copy of the Unit map
        /// </returns>

        public Unit[,] GetAllUnitsAndPositions()
        {
            Unit[,] copy = mUnits.Clone() as Unit[,];
            return copy;
        }

        /// <summary>
        /// Retrieves a copy of the 2D Tile map of the Game
        /// </summary>
        /// <returns>
        /// Returns a copy of the Tile map
        /// </returns>

        public Tile[,] GetAllTilesAndPositions()
        {
            Tile[,] copy = mTiles.Clone() as Tile[,];
            return copy;
        }

        /// <summary>
        /// Applies a GameMove to the Game
        /// </summary>
        /// <param name="move">The GameMove that will be applied</param>

        public void ApplyMove(GameMove move) {

            // Check the Move Type and call the appropriate function to
            // execute move.
            switch (move.MoveType) {
                case GameMove.GameMoveType.Move:
                    MoveUnit(move);
                    break;
                case GameMove.GameMoveType.Attack:
                    AttackUnit(move);
                    break;
                case GameMove.GameMoveType.Skill:
                    SkillUnit(move);
                    break;
                case GameMove.GameMoveType.Item:
                    ItemUnit(move);
                    break;
                case GameMove.GameMoveType.Wait:
                    WaitUnit(move);
                    break;
            }

            // If game hasn't ended yet, check if Current Player has no moves and switch if so
            if (!GameHasEnded && CurrentPlayerHasNoMoves) {
                SwitchPlayer();
            } else if (GameHasEnded) {
                Debug.Log("Game Over");
            }
        }

        /// <summary>
        /// Switch control of the game to the next Player
        /// </summary>

        public void SwitchPlayer() {
            Debug.Log("Switching Player!");

            // Calculate index of next Player
            int index = mPlayers.FindIndex(player => player == CurrentPlayer);
            index = (index + 1) % mPlayers.Count;

            // If index is zero, player cycle restarts and the next "turn" starts
            if (index == 0) {
                Turn++;
            }

            // Set new CurrentPlayer and start their turn
            CurrentPlayer = mPlayers[index];
            CurrentPlayer.StartTurn();
        }

        #region Move Calculations

        /// <summary>
        /// Get all movement locations of a Unit
        /// </summary>
        /// <param name="unit">The Unit to get move locations</param>
        /// <returns>Set of positions that the Unit can move to</returns>

        public HashSet<Vector2Int> GetUnitMoveLocations(Unit unit) {

            var moveLocations = new HashSet<Vector2Int>();

            // Get current unit location and add as possible move location
            var gridPoint = GridForUnit(unit);
            moveLocations.Add(gridPoint);

            // Create temporary distance 2D array of all tiles
            int[,] distance = new int[Columns, Rows];

            // Maximize distances at all locations
            for (int col = 0; col < Columns; col++) {
                for (int row = 0; row < Rows; row++) {
                    distance[col, row] = Int32.MaxValue;
                }
            }

            // Set the Unit's current location distance to 0
            distance[gridPoint.x, gridPoint.y] = 0;

            var tileQueue = new Queue<Tile>();
            var rootTile = mTiles[gridPoint.x, gridPoint.y];
            tileQueue.Enqueue(rootTile);

            while (tileQueue.Count > 0) {
                Tile current = tileQueue.Dequeue();

                // If there is an obstacle or enemy Unit at this tile, then we cannot pass through it.
                // If unit can't move to this tile, check the next tile in the queue.
                if (!unit.CanMove(current) || EnemyAtLocation(current.Position)) {
                    continue;
                }

                // Otherwise, move to this tile and check the neighboring tiles.
                foreach (var neighbor in current.Neighbors) {
                    if (distance[neighbor.Position.x, neighbor.Position.y] > unit.Movement.Value) {
                        int movementCost = unit.MoveCost(neighbor);
                        distance[neighbor.Position.x, neighbor.Position.y] = movementCost + distance[current.Position.x, current.Position.y];

                        if (distance[neighbor.Position.x, neighbor.Position.y] <= unit.Movement.Value) {
                            tileQueue.Enqueue(neighbor);
                        }
                    }
                }

                // If current tile is within moving distance, add it as move location
                if (distance[current.Position.x, current.Position.y] > 0 && distance[current.Position.x, current.Position.y] <= unit.Movement.Value) {
                    moveLocations.Add(current.Position);
                }
            }
            return moveLocations;
        }

        /// <summary>
        /// Gets only the possible locations that a Unit can move to
        /// </summary>
        /// <param name="unit">Unit to get possible locations of</param>
        /// <returns>
        /// Returns Set of filtered positions that the Unit can move to
        /// </returns>

        public HashSet<Vector2Int> GetPossibleUnitMoveLocations(Unit unit) {

            // Filter out locations that are occupied except that of the unit moving
            var gridPoint = GridForUnit(unit);
            var moveLocations = GetUnitMoveLocations(unit)
                                .Where(loc =>
                                    loc == gridPoint
                                    || !TileIsOccupied(loc))
                                    .ToHashSet();
            return moveLocations;
        }

        /// <summary>
        /// Return a key-value pair of positions and cost to move to that position
        /// </summary>
        /// <param name="unit">Unit to get </param>
        /// <returns>
        /// Dictionary of positions to movement costs
        /// </returns>

        public Dictionary<Vector2Int, int> GetUnitMoveCosts(Unit unit) {
            var moveCosts = new Dictionary<Vector2Int, int>();

            var moveLocations = GetUnitMoveLocations(unit);

            foreach (var loc in moveLocations) {
                var tile = mTiles[loc.x, loc.y];

                moveCosts.Add(loc, unit.MoveCost(tile));

            }

            return moveCosts;
        }

        #endregion

        #region Attack Calculations

        /// <summary>
        /// Gets all attack locations that a Unit can attack
        /// </summary>
        /// <param name="unit">Unit to calculate all attack locations</param>
        /// <returns>
        /// Returns Set of positions that Unit can attack overall
        /// </returns>

        public HashSet<Vector2Int> GetUnitAttackLocations(Unit unit) {
            var gridPoint = GridForUnit(unit);
            var moveLocations = GetUnitMoveLocations(unit);

            var attackLocations = new HashSet<Vector2Int>();

            // For each move location, get the surrounding positions you can attack.
            // Add each surrounding position to the list of attack locations if it's not there
            foreach (var moveLoc in moveLocations) {
                var surroundingLocations = GetSurroundingAttackLocationsAtPoint(moveLoc, unit.MainWeapon.Range);
                attackLocations.AddRange(surroundingLocations);
            }

            // Filter the positions that are not occupied or have an enemy at the location
            attackLocations = attackLocations
                                .Where(pos => 
                                    !TileIsOccupied(pos)
                                    || EnemyAtLocation(pos))
                                .ToHashSet();

            return attackLocations;
        }

        /// <summary>
        /// Retrieves only the possible locations that a Unit can attack
        /// </summary>
        /// <param name="unit">Unit to calculate all possible attack locations</param>
        /// <returns>Returns Set of positions that Unit can attack</returns>

        public HashSet<Vector2Int> GetPossibleUnitAttackLocations(Unit unit) {
            var moveLocations = GetPossibleUnitMoveLocations(unit);
            var attackLocations = GetUnitAttackLocations(unit);

            var filteredAttackLocations = new HashSet<Vector2Int>();
            foreach (var loc in attackLocations) {
                var surroundingLocations = GetSurroundingAttackLocationsAtPoint(loc, unit.MainWeapon.Range);
                var attackPoints = surroundingLocations.Where(sLoc => moveLocations.Contains(sLoc));
                if (attackPoints.Count() > 0) {
                    filteredAttackLocations.Add(loc);
                }
            }

            return filteredAttackLocations;
        }

        /// <summary>
        /// Get surrounding attack locations based on some range
        /// </summary>
        /// <param name="attackPoint">Position that attack will be coming from</param>
        /// <param name="range">The range of the attack</param>
        /// <returns>
        /// Returns Set of positions that an attack of some range can attack 
        /// </returns>

        public HashSet<Vector2Int> GetSurroundingAttackLocationsAtPoint(Vector2Int attackPoint, int range) {

            var possibleAttackLocations = new HashSet<Vector2Int> {
                attackPoint
            };

            for (int i = 0; i < range; i++) {
                var tempAttackLocs = new HashSet<Vector2Int>();
                foreach (var attackLoc in possibleAttackLocations) {
                    var attackLocNeighbors = mTiles[attackLoc.x, attackLoc.y]
                        .Neighbors
                        .Select(tile => tile.Position);

                    tempAttackLocs.AddRange(attackLocNeighbors);
                }
                possibleAttackLocations.AddRange(tempAttackLocs);
            }

            return possibleAttackLocations;

        }

        #endregion

        #region Skill Calculations

        /// <summary>
        /// Retrieves skills and usable positions where a Unit can use that skill
        /// </summary>
        /// <param name="unit">Unit to check skill locations</param>
        /// <returns>
        /// Return dictionary of skills and usable locations where a Unit can use the skill
        /// </returns>

        public Dictionary<ActiveSkill, HashSet<Vector2Int>> GetUnitSkillLocations(Unit unit) {
            var skillToLocations = new Dictionary<ActiveSkill, HashSet<Vector2Int>>();

            // Filter out passive skills
            var activeSkills = unit.Skills
                                        .Where(sk => sk is ActiveSkill)
                                        .Select(sk => sk as ActiveSkill);

            // For non-target or multi-target skills, use all move locations
            // TODO: Implement adding other skills

            // For single-target skills, get usable locations

            var singleTargetSkills = activeSkills
                                        .Where(sk => sk is SingleTargetSkill)
                                        .Select(sk => sk as SingleTargetSkill);

            foreach (var singleTargetSkill in singleTargetSkills) {
                var skillLocations = new HashSet<Vector2Int>();
                skillLocations.AddRange(GetUnitSkillLocations(unit, singleTargetSkill));
                skillToLocations.Add(singleTargetSkill, skillLocations);
            }

            return skillToLocations;
        }

        /// <summary>
        /// Retrieves skills and possible positions where a Unit can use that skill
        /// </summary>
        /// <param name="unit">Unit to check skill locations</param>
        /// <returns>
        /// Return dictionary of skills and possible locations where a Unit can use the skill
        /// </returns>

        public Dictionary<ActiveSkill, HashSet<Vector2Int>> GetUnitPossibleSkillLocations(Unit unit) {
            var skillToLocations = new Dictionary<ActiveSkill, HashSet<Vector2Int>>();

            // Filter out passive skills or unusable skills
            var usableActiveSkills = unit.Skills
                                        .Where(sk => sk is ActiveSkill)
                                        .Select(sk => sk as ActiveSkill);

            // For non-target or multi-target skills, use all move locations
            // TODO: Implement adding other skills

            // For single-target skills, get usable locations

            var singleTargetSkills = usableActiveSkills
                                        .Where(sk => sk is SingleTargetSkill)
                                        .Select(sk => sk as SingleTargetSkill);

            foreach (var singleTargetSkill in singleTargetSkills) {
                var skillLocations = new HashSet<Vector2Int>();
                skillLocations.AddRange(GetPossibleUnitSkillLocations(unit, singleTargetSkill));
                skillToLocations.Add(singleTargetSkill, skillLocations);
            }

            return skillToLocations;
        }

        /// <summary>
        /// Get locations where a Unit can use a SingleTargetSkill
        /// </summary>
        /// <param name="unit">Unit to use skill</param>
        /// <param name="skill">SingleTargetSkill that will be used</param>
        /// <returns>
        /// Returns set of positions where a Unit can use a given skill
        /// </returns>

        public HashSet<Vector2Int> GetUnitSkillLocations(Unit unit, SingleTargetSkill skill) {
            var gridPoint = GridForUnit(unit);
            var moveLocations = GetUnitMoveLocations(unit);

            var skillLocations = new HashSet<Vector2Int>();

            foreach (var moveLoc in moveLocations) {
                var surroundingLocations = GetSurroundingAttackLocationsAtPoint(moveLoc, skill.Range);
                skillLocations.AddRange(surroundingLocations);
            }

            skillLocations = skillLocations
                                .Where(pos => 
                                    !TileIsOccupied(pos)
                                    || (skill is SingleDamageSkill && EnemyAtLocation(pos))
                                    || (skill is SingleSupportSkill && AllyAtLocation(pos)))
                                .ToHashSet();

            return skillLocations;
        }

        /// <summary>
        /// Gets possible locations where a Unit can use a SingleTargetSkill
        /// </summary>
        /// <param name="unit">Unit to use skill</param>
        /// <param name="skill">SingleTargetSkill that will be used</param>
        /// <returns>
        /// Returns set of possible positions where a Unit can use a given skill
        /// </returns>

        public HashSet<Vector2Int> GetPossibleUnitSkillLocations(Unit unit, SingleTargetSkill skill) {
            var moveLocations = GetPossibleUnitMoveLocations(unit);
            var skillLocations = GetUnitSkillLocations(unit, skill)
                                    .Where(pos =>
                                        !TileIsOccupied(pos)
                                        || (TileIsOccupied(pos) && skill.IsUsableOnTarget(unit, GetUnitAtPosition(pos))));

            var filteredSkillLocations = new HashSet<Vector2Int>();
            foreach (var loc in skillLocations) {
                var surroundingLocations = GetSurroundingAttackLocationsAtPoint(loc, skill.Range);
                var skillPoints = surroundingLocations.Where(sLoc => moveLocations.Contains(sLoc));
                if (skillPoints.Count() > 0) {
                    filteredSkillLocations.Add(loc);
                }
            }

            return filteredSkillLocations;
        }

        /// <summary>
        /// Gets locations where a Unit can use a SingleDamageSkill
        /// </summary>
        /// <param name="unit">Unit to use skill</param>
        /// <returns>
        /// Returns set of positions that a Unit can use a SingleDamageSkill
        /// </returns>

        public HashSet<Vector2Int> GetUnitDamageSkillLocations(Unit unit) {
            var singleTargetSkills = unit.Skills
                                        .Select(sk => sk as SingleDamageSkill)
                                        .Where(sk => sk != null);

            var skillLocations = new HashSet<Vector2Int>();
            foreach (var skill in singleTargetSkills) {
                skillLocations.AddRange(GetUnitSkillLocations(unit, skill));
            }

            return skillLocations;
        }

        /// <summary>
        /// Gets possible locations where a Unit can use a SingleDamageSkill
        /// </summary>
        /// <param name="unit">Unit to use skill</param>
        /// <returns>
        /// Returns set of possible positions that a Unit can use a SingleDamageSkill
        /// </returns>

        public HashSet<Vector2Int> GetPossibleUnitDamageSkillLocations(Unit unit) {
            var singleTargetSkills = unit.Skills
                                        .Select(sk => sk as SingleDamageSkill)
                                        .Where(sk => sk != null);

            var skillLocations = new HashSet<Vector2Int>();
            foreach (var skill in singleTargetSkills) {
                skillLocations.AddRange(GetPossibleUnitSkillLocations(unit, skill));
            }

            return skillLocations;
        }

        /// <summary>
        /// Gets locations where a Unit can use a SingleSupportSkill
        /// </summary>
        /// <param name="unit">Unit to use skill</param>
        /// <returns>
        /// Returns set of positions that a Unit can use a SingleSupportSkill
        /// </returns>

        public HashSet<Vector2Int> GetUnitSupportSkillLocations(Unit unit) {
            var singleTargetSkills = unit.Skills
                                        .Select(sk => sk as SingleSupportSkill)
                                        .Where(sk => sk != null);

            var skillLocations = new HashSet<Vector2Int>();
            foreach (var skill in singleTargetSkills) {
                skillLocations.AddRange(GetUnitSkillLocations(unit, skill));
            }

            return skillLocations;
        }

        /// <summary>
        /// Gets possible locations where a Unit can use a SingleSupportSkill
        /// </summary>
        /// <param name="unit">Unit to use skill</param>
        /// <returns>
        /// Returns set of possible positions that a Unit can use a SingleSupportSkill
        /// </returns>

        public HashSet<Vector2Int> GetPossibleUnitSupportSkillLocations(Unit unit) {
            var singleTargetSkills = unit.Skills
                                        .Select(sk => sk as SingleSupportSkill)
                                        .Where(sk => sk != null);

            var skillLocations = new HashSet<Vector2Int>();
            foreach (var skill in singleTargetSkills) {
                skillLocations.AddRange(GetPossibleUnitSkillLocations(unit, skill));
            }

            return skillLocations;
        }

        /// <summary>
        /// Determines if a SingleTargetSkill used by a unit is usable on a target unit
        /// </summary>
        /// <param name="usingUnit">The Unit using the Skill</param>
        /// <param name="targetUnit">The target unit affected by the skill</param>
        /// <param name="skill">The Skill that will be used</param>
        /// <returns>
        /// Returns <c>true</c> if the Skill can be used, <c>false</c> otherwise.
        /// </returns>

        public bool SkillIsUsableOnTarget(Unit usingUnit, Unit targetUnit, SingleTargetSkill skill) {
            if (targetUnit != null && usingUnit == targetUnit) {
                return skill.CanTargetSelf && skill.IsUsableOnTarget(usingUnit, usingUnit);
            }
            return targetUnit != null && skill.IsUsableOnTarget(usingUnit, targetUnit);
        }

        #endregion

        #region Shortest Path Calculations

        /// <summary>
        /// Gets the shortest path for a Unit to move from one point to another
        /// </summary>
        /// <param name="unit">The Unit that will be moving</param>
        /// <param name="startPoint">The starting position of the path</param>
        /// <param name="endPoint">The end position of the path</param>
        /// <returns>
        /// Returns list of positions in a path for a Unit to move from the start location to the end location
        /// </returns>

        public List<Vector2Int> GetShortestPath(Unit unit, Vector2Int startPoint, Vector2Int endPoint) {
            var unitCosts = GetUnitMoveCosts(unit);
            var moveGraph = new WeightedGraph(unitCosts);

            var distances = moveGraph.GetShortestDistancesFrom(startPoint);

            var shortestDistanceToEnd = distances.SingleOrDefault(d => d.Vertex == endPoint);

            return shortestDistanceToEnd.Path;
        }
        
        /// <summary>
        /// Gets the shortest path for a Unit to move from one point to the 
        /// point closest to a position where they can attack.
        /// </summary>
        /// <param name="unit">The Unit that will be moving</param>
        /// <param name="startPoint">The starting position of the path</param>
        /// <param name="targetPoint">The position the unit will be attacking</param>
        /// <returns>
        /// Returns list of positions in a path for a Unit to move from the start location to the end location
        /// </returns>

        public List<Vector2Int> GetShortestPathToAttack(Unit unit, Vector2Int startPoint, Vector2Int targetPoint) {
            var unitCosts = GetUnitMoveCosts(unit);
            var moveGraph = new WeightedGraph(unitCosts);

            var availableAttackLocations = GetPossibleUnitMoveLocations(unit);

            var possibleAttackLocations = GetSurroundingAttackLocationsAtPoint(targetPoint, unit.MainWeapon.Range)
                                            .Where(pos => availableAttackLocations.Contains(pos));

            var distances = moveGraph.GetShortestDistancesFrom(startPoint);

            var attackDistances = distances.Where(pos => possibleAttackLocations.Contains(pos.Vertex));

            var shortestDistanceToAttack = attackDistances.Min();

            return shortestDistanceToAttack.Path;
        }

        /// <summary>
        /// Gets the shortest path for a Unit to move from one point to the 
        /// point closest to a position where they can use a Skill.
        /// </summary>
        /// <param name="unit">The Unit that will be moving</param>
        /// <param name="startPoint">The starting position of the path</param>
        /// <param name="targetPoint">The position the unit will be attacking</param>
        /// <returns>
        /// Returns list of positions in a path for a Unit to move from the start location to the end location
        /// </returns>

        public List<Vector2Int> GetShortestPathToSkill(Unit unit, Vector2Int startPoint, Vector2Int targetPoint) {
            var unitCosts = GetUnitMoveCosts(unit);
            var moveGraph = new WeightedGraph(unitCosts);

            var availableAttackLocations = GetUnitMoveLocations(unit);

            var bestSkillRange = unit.Skills.Where(sk => sk is SingleTargetSkill).Select(sk => (sk as SingleTargetSkill).Range).Max();

            var possibleAttackLocations = GetSurroundingAttackLocationsAtPoint(targetPoint, bestSkillRange);

            possibleAttackLocations = possibleAttackLocations.Where(pos =>
                                        (!TileIsOccupied(pos) || GetUnitAtPosition(pos) == unit))
                                        .Where(pos => availableAttackLocations.Contains(pos))
                                        .ToHashSet();

            var distances = moveGraph.GetShortestDistancesFrom(startPoint);

            var attackDistances = distances.Where(pos => possibleAttackLocations.Contains(pos.Vertex));

            var shortestDistanceToAttack = attackDistances.Min();

            return shortestDistanceToAttack.Path;
        }

        /// <summary>
        /// Gets the shortest path for a Unit to move from one point to the 
        /// point closest to a position where they can use a particular Skill.
        /// </summary>
        /// <param name="unit">The Unit that will be moving</param>
        /// <param name="startPoint">The starting position of the path</param>
        /// <param name="targetPoint">The position the unit will be attacking</param>
        /// <param name="skill">Skill that will be used </param>
        /// <returns>
        /// Returns list of positions in a path for a Unit to move from the start location to the end location
        /// </returns>

        public List<Vector2Int> GetShortestPathToSkill(Unit unit, Vector2Int startPoint, Vector2Int targetPoint, SingleTargetSkill skill) {
            var unitCosts = GetUnitMoveCosts(unit);
            var moveGraph = new WeightedGraph(unitCosts);

            var availableAttackLocations = GetUnitMoveLocations(unit);

            var possibleAttackLocations = GetSurroundingAttackLocationsAtPoint(targetPoint, skill.Range);

            possibleAttackLocations = possibleAttackLocations.Where(pos =>
                                        (!TileIsOccupied(pos) || GetUnitAtPosition(pos) == unit))
                                        .Where(pos => availableAttackLocations.Contains(pos))
                                        .ToHashSet();

            var distances = moveGraph.GetShortestDistancesFrom(startPoint);

            var attackDistances = distances.Where(pos => possibleAttackLocations.Contains(pos.Vertex));

            var shortestDistanceToAttack = attackDistances.Min();

            return shortestDistanceToAttack.Path;
        }

        #endregion

        #endregion 

        #region Private Methods

        /// <summary>
        /// Takes in a GameMove to move a unit from one position to another
        /// </summary>
        /// <param name="move">The GameMove that will be used to move the Unit</param>

        private void MoveUnit(GameMove move) {
            var unit = GetUnitAtPosition(move.StartPosition);
            mUnits[move.StartPosition.x, move.StartPosition.y] = null;
            mUnits[move.EndPosition.x, move.EndPosition.y] = unit;
        }

        /// <summary>
        /// Takes in a GameMove to have Unit use an Item at their position
        /// </summary>
        /// <param name="move">The GameMove that will be used to have the Unit use the item</param>

        private void ItemUnit(GameMove move) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes the Unit from the board and marks it as inactive
        /// </summary>
        /// <param name="unit">The Unit that will be killed</param>

        private void KillUnit(Unit unit) {
            var unitOwner = mPlayers.SingleOrDefault(player => player.Units.Contains(unit));
            var unitPos = GridForUnit(unit);

            mUnits[unitPos.x, unitPos.y] = null;

        }


        /// <summary>
        /// Takes in a GameMove to have a Unit attack another Unit
        /// </summary>
        /// <param name="move">
        /// The GameMove that will be used to have a Unit initiate an attack another
        /// </param>

        private void AttackUnit(GameMove move) {
            //since we attack and counterattack, 
            var attackingUnit = GetUnitAtPosition(move.StartPosition);
            var defendingUnit = GetUnitAtPosition(move.EndPosition);

            // Attack Logic Here
            AttackUnit(attackingUnit, defendingUnit);
            var defendingUnitAttackLocations = GetSurroundingAttackLocationsAtPoint(move.EndPosition, defendingUnit.MainWeapon.Range);
            if (defendingUnit.IsAlive && defendingUnitAttackLocations.Contains(move.StartPosition)) //check if unit is alive 
                //TODO CHECK RANGE OF UNIT COUNTER
            {
                Debug.Log("COUNTERATTACKING");
                AttackUnit(defendingUnit, attackingUnit);
            }

            //Debug.Log(attackingUnit.UnitInformation + "\n\n");
            //Debug.Log(defendingUnit.UnitInformation);

            attackingUnit.HasMoved = true;
        }

        /// <summary>
        /// Implementation of having a Unit attack another Unit
        /// </summary>
        /// <param name="attackingUnit">The attacking Unit</param>
        /// <param name="defendingUnit">The defending Unit</param>

        private void AttackUnit(Unit attackingUnit, Unit defendingUnit)
        {
            int hitChance = DamageCalculator.GetHitChance(attackingUnit, defendingUnit);
            Debug.Log("Rolling for hit");
            if (DamageCalculator.DiceRoll(hitChance))
            {
                int critChance = DamageCalculator.GetCritRate(attackingUnit, defendingUnit);
                Debug.Log("Rolling for Crit");
                if (DamageCalculator.DiceRoll(critChance))
                {
                    Debug.Log($"{attackingUnit.Name} crits {defendingUnit.Name}");
                    defendingUnit.HealthPoints = defendingUnit.HealthPoints - DamageCalculator.GetCritDamage(attackingUnit, defendingUnit);
                }
                else
                {
                    Debug.Log($"{attackingUnit.Name} attacks {defendingUnit.Name}");
                    defendingUnit.HealthPoints = defendingUnit.HealthPoints - DamageCalculator.GetDamage(attackingUnit, defendingUnit);
                }

                if (!defendingUnit.IsAlive)
                {
                    Debug.Log($"{defendingUnit.Name} has been defeated");
                    KillUnit(defendingUnit);
                    attackingUnit.GainExperience(defendingUnit);
                }
                else
                {
                    attackingUnit.GainExperience(defendingUnit);
                }


            }
            else
            {
                Debug.Log($"{attackingUnit.Name} missed.");
            }
        }

        /// <summary>
        /// Takes in a GameMove for a Unit to use a Skill
        /// </summary>
        /// <param name="move">The GameMove containing the Skill and position it will be used</param>

        private void SkillUnit(GameMove move)
        {
            var skill = move.UsedSkill;

            var usingUnit = GetUnitAtPosition(move.StartPosition);
            usingUnit.HealthPoints -= skill.SkillCost;

            // Call appropriate methods depending on Skill Type
            if (skill is SingleDamageSkill) {
                ApplySingleDamageSkill(move);
            } else if (skill is SingleSupportSkill) {
                ApplySingleSupportSkill(move);
            }

            usingUnit.HasMoved = true;
        }

        /// <summary>
        /// Takes in a GameMove for a Unit to use a SingleDamageSkill
        /// </summary>
        /// <param name="move">The GameMove containing the Skill and position it will be used</param>

        private void ApplySingleDamageSkill(GameMove move) {
            var attackingUnit = GetUnitAtPosition(move.StartPosition);
            var defendingUnit = GetUnitAtPosition(move.EndPosition);

            var usedSkill = move.UsedSkill as SingleDamageSkill;

            // Attack Logic Here
            Debug.Log($"{attackingUnit.Name} attacks with {usedSkill.SkillName} on {defendingUnit.Name}");
            ApplySingleDamageSkill(attackingUnit, defendingUnit, usedSkill);
            var defendingUnitAttackLocations = GetSurroundingAttackLocationsAtPoint(move.EndPosition, defendingUnit.MainWeapon.Range);
            if (defendingUnit.IsAlive && defendingUnitAttackLocations.Contains(move.StartPosition)) //check if unit is alive 
                                                //TODO CHECK RANGE OF UNIT COUNTER
            {
                Debug.Log($"{defendingUnit.Name} counter-attacks {attackingUnit.Name}");
                //instead of Skill[0] of we probably need selected skill or something
                //attackingUnit.HealthPoints = attackingUnit.HealthPoints - DamageCalculator.GetDamage(defendingUnit, attackingUnit);
                AttackUnit(defendingUnit, attackingUnit);
            } 

            //usedSkill.ApplyDamageSkill(attackingUnit, defendingUnit);

            Debug.Log(attackingUnit.UnitInformation + "\n\n");
            Debug.Log(defendingUnit.UnitInformation);
        }

        private void ApplySingleDamageSkill(Unit attackingUnit, Unit defendingUnit, SingleDamageSkill skill) {
            int hitChance = skill.GetHitChance(attackingUnit, defendingUnit);
            Debug.Log($"Rolling for {skill.SkillName} hit");
            if (DamageCalculator.DiceRoll(hitChance)) {
                int critChance = skill.GetCritRate(attackingUnit, defendingUnit);
                Debug.Log($"Rolling for {skill.SkillName} Crit");
                if (DamageCalculator.DiceRoll(critChance)) {
                    Debug.Log($"{attackingUnit.Name} crits with {skill.SkillName} on {defendingUnit.Name}");
                    defendingUnit.HealthPoints -= skill.GetCritDamage(attackingUnit, defendingUnit);
                } else {
                    Debug.Log($"{attackingUnit.Name} uses {skill.SkillName} on {defendingUnit.Name}");
                    defendingUnit.HealthPoints -= skill.GetDamage(attackingUnit, defendingUnit);

                }

                if (!defendingUnit.IsAlive) {
                    Debug.Log($"{defendingUnit.Name} has been defeated");
                    KillUnit(defendingUnit);
                }

                attackingUnit.GainExperience(defendingUnit);


            } else {
                Debug.Log($"{attackingUnit.Name} missed {skill.SkillName}.");
            }
        }

        /// <summary>
        /// Takes in a GameMove for a Unit to use a SingleDamageSkill
        /// </summary>
        /// <param name="move">The GameMove containing the Skill and position it will be used</param>

        private void ApplySingleSupportSkill(GameMove move) {
            var supportingUnit = GetUnitAtPosition(move.StartPosition);
            var supportedUnit = GetUnitAtPosition(move.EndPosition);

            var skill = move.UsedSkill as SingleSupportSkill;
            skill.ApplySupportSkill(supportingUnit, supportedUnit);


            // Attack Logic Here
            Debug.Log($"{supportingUnit.Name} uses {move.UsedSkill.SkillName} on {supportedUnit.Name}");

            Debug.Log(supportingUnit.UnitInformation + "\n\n");
            Debug.Log(supportedUnit.UnitInformation);
        }

        /// <summary>
        /// Takes in a GameMove for a Unit to wait at their position and end their turn
        /// </summary>
        /// <param name="move">The GameMove for a Unit that will wait</param>

        private void WaitUnit(GameMove move) {
            var unit = GetUnitAtPosition(move.StartPosition);
            unit.HasMoved = true;
        }

        #endregion 

    } 
}
