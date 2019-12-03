using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class gameManagerScript : MonoBehaviour
{
   
    //A lot of the UI does not need to be public, they just are currently if you need to make quick changes in the inspector
    //Changing them to private will not break anything, but you will need to re-enable them to show in the inspector
    [Header("UI GameObjects")]
    public TMP_Text currentTeamUI;
    public Canvas displayWinnerUI;

    public TMP_Text UIunitCurrentHealth;
    public TMP_Text UIunitAttackDamage;
    public TMP_Text UIunitAttackRange;
    public TMP_Text UIunitMoveSpeed;
    public TMP_Text UIunitName;
    public UnityEngine.UI.Image UIunitSprite;

    public Canvas UIunitCanvas;
    public GameObject playerPhaseBlock;
    private Animator playerPhaseAnim;
    private TMP_Text playerPhaseText;
   
    //Raycast for the update for unitHover info
    private Ray ray;
    private RaycastHit hit;
   
    /// The number of teams is hard coded as 2, if there are changes in the future a few of the
    /// functions in this class need to be altered as well to update this change.
   
    public int numberOfTeams = 2;
    public int currentTeam;
    public GameObject unitsOnBoard;

    public GameObject team1;
    public GameObject team2;

    public GameObject unitBeingDisplayed;
    public GameObject tileBeingDisplayed;
    public bool displayingUnitInfo;

    public tileMapScript TMS;

    //Cursor Info for tileMapScript
    public int cursorX;
    public int cursorY;
    //currentTileBeingMousedOver
    public int selectedXTile;
    public int selectedYTile;

    //Variables for unitPotentialMovementRoute
    List<Node> currentPathForUnitRoute;
    List<Node> unitPathToCursor;

    public bool unitPathExists;

    public Material UIunitRoute;
    public Material UIunitRouteCurve;
    public Material UIunitRouteArrow;
    public Material UICursor;

    public int routeToX;
    public int routeToY;

    //This game object is to record the location of the 2 count path when it is reset to 0 this is used to remember what tile to disable
    public GameObject quadThatIsOneAwayFromUnit;

   
    public void Start()
    {
        currentTeam = 0;
        setCurrentTeamUI();
        teamHealthbarColorUpdate();
        displayingUnitInfo = false;
        playerPhaseAnim = playerPhaseBlock.GetComponent<Animator>();
        playerPhaseText = playerPhaseBlock.GetComponentInChildren<TextMeshProUGUI>();
        unitPathToCursor = new List<Node>();
        unitPathExists = false;       
      
        TMS = GetComponent<tileMapScript>();

        
    }
    //2019/10/17 there is a small blink between disable and re-enable for path, its a bit jarring, try to fix it later
    public void Update()
    {
        //Always trying to see where the mouse is pointing
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            //Update cursorLocation and unit appearing in the topLeft
            cursorUIUpdate();
            unitUIUpdate();
            //If the unit is selected we want to highlight the current path with the UI
            if (TMS.selectedUnit != null && TMS.selectedUnit.GetComponent<UnitScript>().getMovementStateEnum(1) == TMS.selectedUnit.GetComponent<UnitScript>().unitMoveState)
            {
                //Check to see if the cursor is in range, we cant show movement outside of range so there is no point if its outside
                if (TMS.selectedUnitMoveRange.Contains(TMS.graph[cursorX, cursorY]))
                {
                    //Generate new path to cursor try to limit this to once per new cursor location or else its too many calculations
                    

                    
                    if (cursorX != TMS.selectedUnit.GetComponent<UnitScript>().x || cursorY != TMS.selectedUnit.GetComponent<UnitScript>().y)
                    {
                        if (!unitPathExists&&TMS.selectedUnit.GetComponent<UnitScript>().movementQueue.Count==0)
                        {
                           
                            unitPathToCursor = generateCursorRouteTo(cursorX, cursorY);
                           
                            routeToX = cursorX;
                            routeToY = cursorY;

                            if (unitPathToCursor.Count != 0)
                            {
                                                                                              
                                for(int i = 0; i < unitPathToCursor.Count; i++)
                                {
                                    int nodeX = unitPathToCursor[i].x;
                                    int nodeY = unitPathToCursor[i].y;

                                    if (i == 0)
                                    {
                                        GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
                                        quadToUpdate.GetComponent<Renderer>().material = UICursor;
                                    }
                                    else if (i!=0 && (i+1)!=unitPathToCursor.Count)
                                    {
                                        //This is used to set the indicator for tiles excluding the first/last tile
                                        setCorrectRouteWithInputAndOutput(nodeX, nodeY,i);
                                    }
                                    else if (i == unitPathToCursor.Count-1)
                                    {
                                        //This is used to set the indicator for the final tile;
                                        setCorrectRouteFinalTile(nodeX, nodeY, i);
                                    }
                                    
                                    TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY].GetComponent<Renderer>().enabled = true;
                                   
                                }
                                    
                            }
                            unitPathExists = true;
                          
                        }
                        
                        else if (routeToX != cursorX || routeToY != cursorY)
                        {
                           
                            if (unitPathToCursor.Count != 0)
                            {
                                for (int i = 0; i < unitPathToCursor.Count; i++)
                                {
                                    int nodeX = unitPathToCursor[i].x;
                                    int nodeY = unitPathToCursor[i].y;

                                    TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY].GetComponent<Renderer>().enabled = false;
                                }
                            }
                            
                            unitPathExists = false;
                        }
                    }
                    else if(cursorX == TMS.selectedUnit.GetComponent<UnitScript>().x && cursorY == TMS.selectedUnit.GetComponent<UnitScript>().y)
                    {
                        
                        TMS.disableUnitUIRoute();
                        unitPathExists = false;
                    }
                    
                }               
            }
        
        }
        
    }
    //In: 
    //Out: void
    //Desc: sets the current player Text in the UI
    public void setCurrentTeamUI()
    {
        currentTeamUI.SetText("Current Player is : Player " + (currentTeam+1).ToString());
    }

    //In: 
    //Out: void
    //Desc: increments the current team
    public void switchCurrentPlayer()
    {
        resetUnitsMovements(returnTeam(currentTeam));
        currentTeam++;
        if (currentTeam == numberOfTeams)
        {
            currentTeam = 0;
        }
        
    }

    //In: int i, the index for each team
    //Out: gameObject team
    //Desc: return the gameObject of the requested team
    public GameObject returnTeam(int i)
    {
        GameObject teamToReturn = null;
        if (i == 0)
        {
            teamToReturn = team1;
        }
        else if (i == 1)
        {
            teamToReturn = team2;
        }
        return teamToReturn;
    }

    //In: gameObject team - used to reset (re-enable) all the unit movements
    //Out: void
    //Desc: re-enables movement for all units on the team
    public void resetUnitsMovements(GameObject teamToReset)
    {
        foreach (Transform unit in teamToReset.transform)
        {
            unit.GetComponent<UnitScript>().moveAgain();
        }
    }

    //In: 
    //Out: void
    //Desc: ends the turn and plays the animation
    public void endTurn()
    {
        
        if (TMS.selectedUnit == null)
        {
            switchCurrentPlayer();
            if (currentTeam == 1)
            {
                playerPhaseAnim.SetTrigger("slideLeftTrigger");
                playerPhaseText.SetText("Player 2 Phase");
            }
            else if (currentTeam == 0)
            {
                playerPhaseAnim.SetTrigger("slideRightTrigger");
                playerPhaseText.SetText("Player 1 Phase");
            }
            teamHealthbarColorUpdate();
            setCurrentTeamUI();
        }
    }

    //In: attacking unit and receiving unit
    //Out: void
    //Desc: checks to see if units remain on a team
    public void checkIfUnitsRemain(GameObject unit, GameObject enemy)
    {
        //  Debug.Log(team1.transform.childCount);
        //  Debug.Log(team2.transform.childCount);
        StartCoroutine(checkIfUnitsRemainCoroutine(unit,enemy));
    }


    //In:
    //Out: void
    //Desc: updates the cursor for the UI
    public void cursorUIUpdate()
    {
       //If we are mousing over a tile, highlight it
        if (hit.transform.CompareTag("Tile"))
        {
            if (tileBeingDisplayed == null)
            {
                selectedXTile = hit.transform.gameObject.GetComponent<ClickableTileScript>().tileX;
                selectedYTile = hit.transform.gameObject.GetComponent<ClickableTileScript>().tileY;
                cursorX = selectedXTile;
                cursorY = selectedYTile;
                TMS.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = true;
                tileBeingDisplayed = hit.transform.gameObject;
                
            }
            else if (tileBeingDisplayed != hit.transform.gameObject)
            {
                selectedXTile = tileBeingDisplayed.GetComponent<ClickableTileScript>().tileX;
                selectedYTile = tileBeingDisplayed.GetComponent<ClickableTileScript>().tileY;
                TMS.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = false;

                selectedXTile = hit.transform.gameObject.GetComponent<ClickableTileScript>().tileX;
                selectedYTile = hit.transform.gameObject.GetComponent<ClickableTileScript>().tileY;
                cursorX = selectedXTile;
                cursorY = selectedYTile;
                TMS.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = true;
                tileBeingDisplayed = hit.transform.gameObject;
                
            }

        }
        //If we are mousing over a unit, highlight the tile that the unit is occupying
        else if (hit.transform.CompareTag("Unit"))
        {
            if (tileBeingDisplayed == null)
            {
                selectedXTile = hit.transform.parent.gameObject.GetComponent<UnitScript>().x;
                selectedYTile = hit.transform.parent.gameObject.GetComponent<UnitScript>().y;
                cursorX = selectedXTile;
                cursorY = selectedYTile;
                TMS.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = true;
                tileBeingDisplayed = hit.transform.parent.gameObject.GetComponent<UnitScript>().tileBeingOccupied;

            }
            else if (tileBeingDisplayed != hit.transform.gameObject)
            {
                if (hit.transform.parent.gameObject.GetComponent<UnitScript>().movementQueue.Count == 0)
                {
                    selectedXTile = tileBeingDisplayed.GetComponent<ClickableTileScript>().tileX;
                    selectedYTile = tileBeingDisplayed.GetComponent<ClickableTileScript>().tileY;
                    TMS.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = false;

                    selectedXTile = hit.transform.parent.gameObject.GetComponent<UnitScript>().x;
                    selectedYTile = hit.transform.parent.gameObject.GetComponent<UnitScript>().y;
                    cursorX = selectedXTile;
                    cursorY = selectedYTile;
                    TMS.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = true;
                    tileBeingDisplayed = hit.transform.parent.GetComponent<UnitScript>().tileBeingOccupied;
                   
                }
               
            }
        }
        //We aren't pointing at anything no cursor.
        else
        {
            TMS.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = false;
        }
    }


    //In: 
    //Out: void
    //Desc: the unit that is being highlighted will have its stats in the UI
    public void unitUIUpdate()
    {
        if (!displayingUnitInfo)
        {
            if (hit.transform.CompareTag("Unit"))
            {
                UIunitCanvas.enabled = true;
                displayingUnitInfo = true;
                unitBeingDisplayed = hit.transform.parent.gameObject;
                var highlightedUnitScript = hit.transform.parent.gameObject.GetComponent<UnitScript>();

                UIunitCurrentHealth.SetText(highlightedUnitScript.currentHealthPoints.ToString());
                UIunitAttackDamage.SetText(highlightedUnitScript.attackDamage.ToString());
                UIunitAttackRange.SetText(highlightedUnitScript.attackRange.ToString());
                UIunitMoveSpeed.SetText(highlightedUnitScript.moveSpeed.ToString());
                UIunitName.SetText(highlightedUnitScript.unitName);
                UIunitSprite.sprite = highlightedUnitScript.unitSprite;
                
            }
            else if (hit.transform.CompareTag("Tile"))
            {
                if (hit.transform.GetComponent<ClickableTileScript>().unitOnTile != null)
                {
                    unitBeingDisplayed = hit.transform.GetComponent<ClickableTileScript>().unitOnTile;

                    UIunitCanvas.enabled = true;
                    displayingUnitInfo = true;
                    var highlightedUnitScript = unitBeingDisplayed.GetComponent<UnitScript>();

                    UIunitCurrentHealth.SetText(highlightedUnitScript.currentHealthPoints.ToString());
                    UIunitAttackDamage.SetText(highlightedUnitScript.attackDamage.ToString());
                    UIunitAttackRange.SetText(highlightedUnitScript.attackRange.ToString());
                    UIunitMoveSpeed.SetText(highlightedUnitScript.moveSpeed.ToString());
                    UIunitName.SetText(highlightedUnitScript.unitName);
                    UIunitSprite.sprite = highlightedUnitScript.unitSprite;

                }
            }
        }
        else if (hit.transform.gameObject.CompareTag("Tile"))
        {
            if (hit.transform.GetComponent<ClickableTileScript>().unitOnTile == null)
            {
                UIunitCanvas.enabled = false;
                displayingUnitInfo = false;
            }
            else if (hit.transform.GetComponent<ClickableTileScript>().unitOnTile != unitBeingDisplayed)
            {
                UIunitCanvas.enabled = false;
                displayingUnitInfo = false;
            }
        }
        else if (hit.transform.gameObject.CompareTag("Unit"))
        {
            if (hit.transform.parent.gameObject != unitBeingDisplayed)
            {
                UIunitCanvas.enabled = false;
                displayingUnitInfo = false;
            }
        }
    }

    //In: 
    //Out: void
    //Desc: When the current team is active, the healthbars are blue, and the other team is red
    public void teamHealthbarColorUpdate()
    {
        for(int i = 0; i < numberOfTeams; i++)
        {
            GameObject team = returnTeam(i);
            if(team == returnTeam(currentTeam))
            {
                foreach (Transform unit in team.transform)
                {
                    unit.GetComponent<UnitScript>().changeHealthBarColour(0);
                }
            }
            else
            {
                foreach (Transform unit in team.transform)
                {
                    unit.GetComponent<UnitScript>().changeHealthBarColour(1);
                }
            }
        }
       
        
    }
    //In: x and y location to go to
    //Out: list of nodes to traverse
    //Desc: generate the cursor route to a position x , y
    public List<Node> generateCursorRouteTo(int x, int y)
    {

        if (TMS.selectedUnit.GetComponent<UnitScript>().x == x && TMS.selectedUnit.GetComponent<UnitScript>().y == y)
        {
            Debug.Log("clicked the same tile that the unit is standing on");
            currentPathForUnitRoute = new List<Node>();
            

            return currentPathForUnitRoute;
        }
        if (TMS.unitCanEnterTile(x, y) == false)
        {
            //cant move into something so we can probably just return
            //cant set this endpoint as our goal

            return null;
        }

        //TMS.selectedUnit.GetComponent<UnitScript>().path = null;
        currentPathForUnitRoute = null;
        //from wiki dijkstra's
        Dictionary<Node, float> dist = new Dictionary<Node, float>();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();
        Node source = TMS.graph[TMS.selectedUnit.GetComponent<UnitScript>().x, TMS.selectedUnit.GetComponent<UnitScript>().y];
        Node target = TMS.graph[x, y];
        dist[source] = 0;
        prev[source] = null;
        //Unchecked nodes
        List<Node> unvisited = new List<Node>();

        //Initialize
        foreach (Node n in TMS.graph)
        {

            //Initialize to infite distance as we don't know the answer
            //Also some places are infinity
            if (n != source)
            {
                dist[n] = Mathf.Infinity;
                prev[n] = null;
            }
            unvisited.Add(n);
        }
        //if there is a node in the unvisited list lets check it
        while (unvisited.Count > 0)
        {
            //u will be the unvisited node with the shortest distance
            Node u = null;
            foreach (Node possibleU in unvisited)
            {
                if (u == null || dist[possibleU] < dist[u])
                {
                    u = possibleU;
                }
            }


            if (u == target)
            {
                break;
            }

            unvisited.Remove(u);

            foreach (Node n in u.neighbours)
            {

                //float alt = dist[u] + u.DistanceTo(n);
                float alt = dist[u] + TMS.costToEnterTile(n.x, n.y);
                if (alt < dist[n])
                {
                    dist[n] = alt;
                    prev[n] = u;
                }
            }
        }
        //if were here we found shortest path, or no path exists
        if (prev[target] == null)
        {
            //No route;
            return null;
        }
        currentPathForUnitRoute = new List<Node>();
        Node curr = target;
        //Step through the current path and add it to the chain
        while (curr != null)
        {
            currentPathForUnitRoute.Add(curr);
            curr = prev[curr];
        }
        //Now currPath is from target to our source, we need to reverse it from source to target.
        currentPathForUnitRoute.Reverse();

        return currentPathForUnitRoute;




    }

    //In: gameObject quad 
    //Out: void
    //Desc: reset its rotation
    public void resetQuad(GameObject quadToReset)
    {
        quadToReset.GetComponent<Renderer>().material = UICursor;
        quadToReset.transform.eulerAngles = new Vector3(90, 0, 0);
        
    }

    //In: Vector2 cursorPos the location we change, Vector3 the rotation that we will rotate the quad
    //Out: void
    //Desc: the quad is rotated approriately
    public void UIunitRouteArrowDisplay(Vector2 cursorPos,Vector3 arrowRotationVector)
    {
        GameObject quadToManipulate = TMS.quadOnMapForUnitMovementDisplay[(int)cursorPos.x, (int)cursorPos.y];
        quadToManipulate.transform.eulerAngles = arrowRotationVector;
        quadToManipulate.GetComponent<Renderer>().material = UIunitRouteArrow;
        quadToManipulate.GetComponent<Renderer>().enabled = true;
    }

    //In: two gameObjects current vector and the next one in the list
    //Out: vector which is the direction between the two inputs
    //Desc: the direction from current to the next vector is returned
    public Vector2 directionBetween(Vector2 currentVector, Vector2 nextVector)
    {

        
        Vector2 vectorDirection = (nextVector - currentVector).normalized;
       
        if (vectorDirection == Vector2.right)
        {
            return Vector2.right;
        }
        else if (vectorDirection == Vector2.left)
        {
            return Vector2.left;
        }
        else if (vectorDirection == Vector2.up)
        {
            return Vector2.up;
        }
        else if (vectorDirection == Vector2.down)
        {
            return Vector2.down;
        }
        else
        {
            Vector2 vectorToReturn = new Vector2();
            return vectorToReturn;
        }
    }

    //In: two nodes that are being checked and int i is the position in the path ie i=0 is the first thing in the list
    //Out: void
    //Desc: orients the quads to display proper information
    public void setCorrectRouteWithInputAndOutput(int nodeX,int nodeY,int i)
    {
        Vector2 previousTile = new Vector2(unitPathToCursor[i - 1].x + 1, unitPathToCursor[i - 1].y + 1);
        Vector2 currentTile = new Vector2(unitPathToCursor[i].x + 1, unitPathToCursor[i].y + 1);
        Vector2 nextTile = new Vector2(unitPathToCursor[i + 1].x + 1, unitPathToCursor[i + 1].y + 1);

        Vector2 backToCurrentVector = directionBetween(previousTile, currentTile);
        Vector2 currentToFrontVector = directionBetween(currentTile, nextTile);


        //Right (UP/DOWN/RIGHT)
        if (backToCurrentVector == Vector2.right && currentToFrontVector == Vector2.right)
        {
            //Debug.Log("[IN[R]]->[Out[R]]");
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 270);
            quadToUpdate.GetComponent<Renderer>().material = UIunitRoute;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrentVector == Vector2.right && currentToFrontVector == Vector2.up)
        {
            //Debug.Log("[IN[R]]->[Out[UP]]");
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 180);
            quadToUpdate.GetComponent<Renderer>().material = UIunitRouteCurve;
            quadToUpdate.GetComponent<Renderer>().enabled = true;

        }
        else if (backToCurrentVector == Vector2.right && currentToFrontVector == Vector2.down)
        {
            //Debug.Log("[IN[R]]->[Out[DOWN]]");
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 270);
            quadToUpdate.GetComponent<Renderer>().material = UIunitRouteCurve;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        //Left (UP/DOWN/LEFT)
        else if (backToCurrentVector == Vector2.left && currentToFrontVector == Vector2.left)
        {
            //Debug.Log("[IN[L]]->[Out[L]]");
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 90);
            quadToUpdate.GetComponent<Renderer>().material = UIunitRoute;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrentVector == Vector2.left && currentToFrontVector == Vector2.up)
        {
            //Debug.Log("[IN[L]]->[Out[UP]]");
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 90);
            quadToUpdate.GetComponent<Renderer>().material = UIunitRouteCurve;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrentVector == Vector2.left && currentToFrontVector == Vector2.down)
        {
            //Debug.Log("[IN[L]]->[Out[DOWN]]");
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 0);
            quadToUpdate.GetComponent<Renderer>().material = UIunitRouteCurve;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        //UP (UP/RIGHT/LEFT)
        else if (backToCurrentVector == Vector2.up && currentToFrontVector == Vector2.up)
        {
            //Debug.Log("[IN[UP]]->[Out[UP]]");
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 0);
            quadToUpdate.GetComponent<Renderer>().material = UIunitRoute;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrentVector == Vector2.up && currentToFrontVector == Vector2.right)
        {
            //Debug.Log("[IN[UP]]->[Out[R]]");
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 0);
            quadToUpdate.GetComponent<Renderer>().material = UIunitRouteCurve;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrentVector == Vector2.up && currentToFrontVector == Vector2.left)
        {
            //Debug.Log("[IN[UP]]->[Out[L]]");
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 270);
            quadToUpdate.GetComponent<Renderer>().material = UIunitRouteCurve;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        //DOWN (DOWN/RIGHT/LEFT)
        else if (backToCurrentVector == Vector2.down && currentToFrontVector == Vector2.down)
        {
            //Debug.Log("[IN[DOWN]]->[Out[DOWN]]");
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 0);
            quadToUpdate.GetComponent<Renderer>().material = UIunitRoute;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrentVector == Vector2.down && currentToFrontVector == Vector2.right)
        {
            //Debug.Log("[IN[DOWN]]->[Out[R]]");
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 90);
            quadToUpdate.GetComponent<Renderer>().material = UIunitRouteCurve;
            quadToUpdate.GetComponent<Renderer>().enabled = true;

        }
        else if (backToCurrentVector == Vector2.down && currentToFrontVector == Vector2.left)
        {
            //Debug.Log("[IN[DOWN]]->[Out[L]]");
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 180);
            quadToUpdate.GetComponent<Renderer>().material = UIunitRouteCurve;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
    }

    //In: two nodes that are being checked and int i is the position in the path ie i=0 is the first thing in the list
    //Out: void
    //Desc: orients the quad for the final node in list to display proper information
    public void setCorrectRouteFinalTile(int nodeX,int nodeY,int i)
    {
        Vector2 previousTile = new Vector2(unitPathToCursor[i - 1].x + 1, unitPathToCursor[i - 1].y + 1);
        Vector2 currentTile = new Vector2(unitPathToCursor[i].x + 1, unitPathToCursor[i].y + 1);
        Vector2 backToCurrentVector = directionBetween(previousTile, currentTile);

        if (backToCurrentVector == Vector2.right)
        {
            //Debug.Log("[IN[R]]");
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 270);
            quadToUpdate.GetComponent<Renderer>().material = UIunitRouteArrow;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrentVector == Vector2.left)
        {
            //Debug.Log("[IN[L]]");
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 90);
            quadToUpdate.GetComponent<Renderer>().material = UIunitRouteArrow;
            quadToUpdate.GetComponent<Renderer>().enabled = true;

        }
        else if (backToCurrentVector == Vector2.up)
        {
            //Debug.Log("[IN[U]]");
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 0);
            quadToUpdate.GetComponent<Renderer>().material = UIunitRouteArrow;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
        else if (backToCurrentVector == Vector2.down)
        {
            //Debug.Log("[IN[D]]");
            GameObject quadToUpdate = TMS.quadOnMapForUnitMovementDisplay[nodeX, nodeY];
            quadToUpdate.GetComponent<Transform>().rotation = Quaternion.Euler(90, 0, 180);
            quadToUpdate.GetComponent<Renderer>().material = UIunitRouteArrow;
            quadToUpdate.GetComponent<Renderer>().enabled = true;
        }
    }

    //In: two units that last fought
    //Out: void
    //Desc: waits until all the animations and stuff are finished before calling the game
    public IEnumerator checkIfUnitsRemainCoroutine(GameObject unit, GameObject enemy)
    {
        while (unit.GetComponent<UnitScript>().combatQueue.Count != 0)
        {
            yield return new WaitForEndOfFrame();
        }
        
        while (enemy.GetComponent<UnitScript>().combatQueue.Count != 0)
        {
            yield return new WaitForEndOfFrame();
        }
        if (team1.transform.childCount == 0)
        {
            displayWinnerUI.enabled = true;
            displayWinnerUI.GetComponentInChildren<TextMeshProUGUI>().SetText("Player 2 has won!");
           
            
        }
        else if (team2.transform.childCount == 0)
        {
            displayWinnerUI.enabled = true;
            displayWinnerUI.GetComponentInChildren<TextMeshProUGUI>().SetText("Player 1 has won!");

          
        }


    }


    //In: 
    //Out: void
    //Desc: set the player winning
    
    public void win()
    {
        displayWinnerUI.enabled = true;
        displayWinnerUI.GetComponentInChildren<TextMeshProUGUI>().SetText("Winner!");

    }

  
   
}
