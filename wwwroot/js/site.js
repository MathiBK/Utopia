let currentHexX = 24;
let currentHexY = 23;
let currentPlayerTileId;
let visibleTiles = [];
getVisibleTiles();
function getMap()
{
     fetch("/api/MapApi")
         .then(response => response.json())
         .then(data => renderMap(data))
         .catch(error => console.error('Unable to get items.', error));
}

function getCurrentCoords()
{
     return {x: currentHexX, y: currentHexY};
}

function renderMap(mapData)
{
     currentPlayerTileId = WhichTiles();
     let canvas = new fabric.Canvas("c");
     $("#c").fabric = canvas;
     let can = $("#c");
     const h = $( "#h" );
     const tileName = $( "#TileName" );
     canvas.selection = false;
     canvas.selectable = false;
     canvas.fireRightClick = true;
     canvas.stopContextMenu = true;
     canvas.renderOnAddRemove = false;


     let tileDataList = [];
     fetch("/api/TileTypeApi")
         .then(response => response.json())
         .then(
             tileData => {

                  tileData.forEach(item => {
                       tileDataList.push(item);
                  });
                  console.log(tileDataList);
             })
         .then( t => {

              mapData.forEach(item => {
                   let tileType = tileDataList[item.tileTypeId - 1].tileTypeName;
                   let color = "#0c4900";
                   switch (tileType)
                   {
                        case "OCEAN":
                             color = "#0b64f3";
                             break;
                        case "LAKE":
                             color = "#36bae3";
                             break;
                        case "BEACH":
                             color = "#e7d789";
                             break;
                        case "SNOW":
                             color = "#fdfcfc";
                             break;
                        case "ICE":
                             color = "#82d5ee";
                             break;
                        case "TROPICAL_SEASONAL_FOREST":
                             color = "#8d7a42";
                             break;
                        case "SUBTROPICAL_DESERT":
                             color = "#d58023";
                             break;
                        case "TUNDRA":
                             color ="#47fac1" ;
                             break;
                        case "BARE":
                             color = "#656865";
                             break;
                        case "SCORCHED":
                             color = "#f8856c";
                             break;
                        case "TAIGA":
                             color = "#7daa50";
                             break;
                        case "SHRUBLAND":
                             color = "#436f49";
                             break;
                        case "TEMPERATE_DESERT":
                             color = "#eac261";
                             break;
                        case "TEMPERATE_RAIN_FOREST":
                             color = "#265000";
                             break;
                        case "TEMPERATE_DECIDUOUS_FOREST":
                             color = "#155e32";
                             break;
                        case "GRASSLAND":
                             color = "#3d5820";
                             break;
                        case "TROPICAL_RAIN_FOREST":
                             color = "#0f840b";
                             break;
                        case "MARSH":
                             color = "#203603";
                             break;
                        default:
                             color = "#bf3ace";
                             break;

                   }

                   let cords = ConvertFromHexCoords(item.hexCordX, item.hexCordY, 10);

                   let points = ComputeHexVerticesFromFace(cords.x, canvas.height - cords.y, 10);

                   let polyTemp = new fabric.Polygon(points, {

                        hasControls: false,
                        hasRotatingPoint: false,
                        selectable: false,
                        hasBorders: false,


                        fill: color,
                        stroke: '#000000',
                        strokeWidth: 0.35,
                        scaleX: 1,
                        scaleY: 1,
                        borderScaleFactor: 5,
                        borderColor: 'white',
                        padding: 4,
                        objectCaching: false,
                        transparentCorners: false,
                        cornerColor: 'black',
                        lockMovementX: true,
                        lockMovementY: true,
                        my: {id: item.id,
                             name: "ting",
                             color: color,
                             hexCoordX: item.hexCordX,
                             hexCoordY: item.hexCordY,
                             foggy: true,
                             draw: true,
                             type: "HEX"
                        }
                   });
                   polyTemp.my.name = tileType;

                   if(item.villageHere == true)
                   {
                        console.log("village");
                        let villageIcon = new fabric.Text("v",
                            {
                                 hasControls: false,
                                 hasRotatingPoint: false,
                                 selectable: false,
                                 hasBorders: false,
                                 lockMovementX: true,
                                 lockMovementY: true,
                                 scaleX: 0.65,
                                 scaleY: 0.65,
                                 left: points[0].x + 5,
                                 top: points[3].y - 5,
                                 originX: "center",
                                 objectCaching: false,
                                 my: {name: "villageIcon",
                                      type: "ICON"}

                            });
                        //canvas.add(villageIcon);
                   }

                   canvas.add(polyTemp);
              });
              let currrentTileText = new fabric.Text("c", {
                   hasControls: false,
                   hasRotatingPoint: false,
                   selectable: false,
                   hasBorders: false,
                   lockMovementX: true,
                   lockMovementY: true,
                   scaleX: 0.65,
                   scaleY: 0.65,

                   originX: "center",
                   objectCaching: false,
                   my: {name: "currentTile",
                        type: "ICON"}

              });
              canvas.add(currrentTileText);
         })
         .then()
         .catch(error => console.error('Unable to get items.', error));

     canvas.on('mouse:wheel', function(opt) {
          console.log(opt.e)
          var delta = -opt.e.deltaY;
          var zoom = canvas.getZoom();
          zoom = zoom + delta/200;
          if (zoom > 5) zoom = 5;
          if (zoom < 1) zoom = 1;
          canvas.zoomToPoint({ x: opt.e.offsetX, y: opt.e.offsetY }, zoom);
          opt.e.preventDefault();
          opt.e.stopPropagation();
          var vpt = this.viewportTransform;
          if (zoom < 400 / 1000) {
               this.viewportTransform[4] = 200 - 1000 * zoom / 2;
               this.viewportTransform[5] = 200 - 1000 * zoom / 2;
          } else {
               if (vpt[4] >= 0) {
                    this.viewportTransform[4] = 0;
               } else if (vpt[4] < canvas.getWidth() - 1000 * zoom) {
                    this.viewportTransform[4] = canvas.getWidth() - 1000 * zoom;
               }
               if (vpt[5] >= 0) {
                    this.viewportTransform[5] = 0;
               } else if (vpt[5] < canvas.getHeight() - 1000 * zoom) {
                    this.viewportTransform[5] = canvas.getHeight() - 1000 * zoom;
               }
          }
          canvas.requestRenderAll();
     });

     canvas.on('mouse:down', function(opt) {
          var evt = opt.e;
          if (opt.button === 3) {
               this.isDragging = true;
               this.lastPosX = evt.clientX;
               this.lastPosY = evt.clientY;
          }

     });
     canvas.on('mouse:move', function(opt) {
          if (this.isDragging) {
               let e = opt.e;
               this.viewportTransform[4] += e.clientX - this.lastPosX;
               this.viewportTransform[5] += e.clientY - this.lastPosY;
               this.requestRenderAll();
               this.lastPosX = e.clientX;
               this.lastPosY = e.clientY;
          }
     });
     canvas.on('mouse:up', function(opt) {
          this.isDragging = false;
          for (let object of canvas.getObjects()) {
               object.setCoords();
          }
     });


     canvas.on('mouse:over', function(e) {
          /*e.target.set('fill', '#379c37');
          canvas.renderAll();*/
     });
     canvas.on('mouse:out', function(e) {
          //canvas.renderAll();
     });


     canvas.on('mouse:down', function(e) {
          if(e.button === 1) {
               if (e.target.my.type === "HEX") {
                    if (e.target.my.name != "OCEAN" && e.target.my.name != "LAKE") {
                         currentHexX = e.target.my.hexCoordX;
                         currentHexY = e.target.my.hexCoordY;
                    }

                    this.selection = false;
                    h.html("x: " + e.target.left + " y: " + e.target.top);
                    tileName.html("Biome: " + e.target.my.name);
                    console.log(e.target.my);
                    e.target.stroke = "#880E4F";
                    e.target.strokeWidth = 2;
                    
                    //GetTileInfo(e.target.my.id).then(t=> console.log(t));

                    let y = canvas.findTarget(e, false);
                    let u = canvas.getObjects();


                    let playerHexX = 0;
                    let playerHexY = 0;

                    for (let obj in u) {
                         if (u[obj].my.type === "HEX") {
                              u[obj].stroke = '#000000';
                              u[obj].strokeWidth = 0.35;
                              if (u[obj].my.id === currentPlayerTileId) {
                                   playerHexX = u[obj].my.hexCoordX;
                                   playerHexY = u[obj].my.hexCoordY;
                                   u[obj].my.foggy = false;
                              }
                         }
                    }

                    e.target.fill = e.target.my.color;
                    if (e.target.my.name !== "OCEAN" && e.target.my.name !== "LAKE") {
                         if (e.target.my.type === "HEX") {
                              let dist = Distance(e.target.my.hexCoordX, e.target.my.hexCoordY, playerHexX, playerHexY);
                              if (dist <= 1) {
                                   setTileInfo(playerHexX, playerHexY, e.target.my.hexCoordX, e.target.my.hexCoordY, e.target.my.name);
                                   e.target.stroke = '#fe5678';
                                   e.target.strokeWidth = 1;
                              }
                         }
                    }
                    e.target.bringToFront();

                    canvas.renderAll();
               }
          }

     });
     
     function ShowVisibleTiles()
     {
          let u = canvas.getObjects();

          let playerHexX = 0;
          let playerHexY = 0;
          let playerTileObj;

          for(let obj in u)
          {
               if(u[obj].my.type === "HEX")
               {
                    if(u[obj].foggy)
                         u[obj].fill = "#fafafa";
                    if(u[obj].my.id === currentPlayerTileId)
                    {
                         playerHexX = u[obj].my.hexCoordX;
                         playerHexY = u[obj].my.hexCoordY;
                         u[obj].my.foggy = false;
                         playerTileObj = u[obj];
                    }
               }
          }

          for(let obj in u)
          {
               if(u[obj].my.type === "HEX")
               {
                    let contains = false;
                    for(let x = 0; x < visibleTiles.length; x++){
                         if(visibleTiles[x] === u[obj].my.id){
                              contains = true;
                         }
                    }
                    if(contains)
                    {
                         u[obj].fill = u[obj].my.color;
                         u[obj].visible = true;
                         u[obj].my.foggy = false;
                    }
                    else {
                         if (u[obj].my.foggy) {
                              u[obj].my.draw = false;
                              u[obj].visible = false;
                         }
                    }
               } else if(u[obj].my.type === "ICON")
               {
                    if(u[obj].my.name === "currentTile")
                    {
                         u[obj].left = playerTileObj.left + (playerTileObj.width/2);
                         u[obj].top = playerTileObj.top - (playerTileObj.height/2);
                    }
                    u[obj].bringToFront();
               }
          }
          canvas.renderAll();
     }
     setInterval(ShowVisibleTiles, 500);

     return canvas;
}

function setTileInfo(originHexX, originHexY, otherHexX, otherHexY, tileType)
{
     
     let direction = 0;
     if(originHexX -otherHexX > 0 && originHexY - otherHexY < 0)
     {
          console.log("NorthWest");
          
          direction = 5;
     } else if (originHexX -otherHexX > 0 && originHexY - otherHexY === 0) {
          console.log("SouthWest");
          direction = 4;
     }
     else if (originHexX -otherHexX === 0 && originHexY - otherHexY < 0) {
          console.log("North");
          
          direction = 0;
     }
     else if (originHexX -otherHexX < 0 && originHexY - otherHexY === 0) {
          console.log("NorthEast");
          direction = 1;
     }
     else if (originHexX -otherHexX < 0 && originHexY - otherHexY > 0) {
          
          console.log("SouthEast");
          direction = 2 ;
     }
     else  {
          console.log("South");
          direction = 3 ;
     }


     let data = "<div class=\"container\">\n" +
     "    <h5>Move ";
     switch (direction) 
     {
          case 0:
               data += "north?</h5>\n";
               break;
          case 1:
               data += "north-east?</h5>\n";
               
               break;
          case 2:
               data += "south-east?</h5>\n";
               break;
          case 3:
               data += "south?</h5>\n";
               break;
          case 4:
               data += "south-west?</h5>\n";
               break;
          case 5:
               data += "north-west?</h5>\n";
               break;
               
               
     }
         data += "    <p>You see:\n" + tileType + "</p>" +
         "</div>"
     
     $("#Mapdata").html(data);
}

async function GetTileInfo(id) {
     await $.ajax({
          url: "/Home/TileInfo",
          data: "id=" + id,
          type: "GET",
          success: function(data)
          {
               $("#Mapdata").html(data);

          },
          error: function(passParams)
          {
               console.log(passParams)
          }
     });
}

async function WhichTiles() {
     $.ajax({
          url: "/Home/CurrentPlayerTile",

          type: "GET",
          success: function(data)
          {
               currentPlayerTileId = data;

          },
          error: function(passParams)
          {
               console.log(passParams)
          }
     });
}

async function getVisibleTiles(){
     await $.ajax({
          url: "/Home/GetVisibleTiles",

          type: "GET",
          success: function(data) {
               for (let x = 0; x < data.length; x++) {
                    visibleTiles[x] = data[x];
               }
               console.log(visibleTiles);
          },
               error: function(passParams)
               {
                    console.log(passParams)
               }
     });
}

function ResGather(name)
{
     $.ajax({
          url: "/Home/ResGather",

          data: "resourceName=" + name,
          type: "POST",
          success: function (data) {

          },
          error: function (passParams) {
               console.log(passParams)
          }
     });
}


function ItemCraft(name)
{
     //Denne skal kobles til egen knapp

     $.ajax({
          url: "/Home/ItemCraft",

          data: "itemName=" + name,
          type: "POST",
          success: function(data)
          {
               $("#resourcesData").html(data);
               console.log(data);

          },
          error: function(passParams)
          {
               console.log(passParams)
          }
     });
}

function Hunting(start)
{
     $.ajax({
          url: "/Home/Hunting",

          data: "start=" + start,
          type: "POST",
          success: function(data)
          {
               console.log(data);
          },
          error: function(passParams)
          {
               console.log(passParams)
          }
     });
}

function FetchPossibleHunt()
{
     $.ajax({
          url: "/Home/FetchPossibleHunt",
          type: "GET",
          success: function(data)
          {
               $("#HuntData").html(data);
               console.log(data);
          },
          error: function(passParams)
          {
               console.log(passParams)
          }
     });
}


function FetchPlayerResources()
{
     console.log("Fetching resources");
     $.ajax({
          url: "/Home/FetchPlayerResources",
          type: "GET",
          success: function(data)
          {
               data = JSON.parse(data);
               console.log("Data");
               console.log(data);
               for (const messageKey in data)
               {
                    console.log(messageKey);
                    console.log(data[messageKey]);
                    $("#" + messageKey + "Name").html(messageKey);
                    $("#" + messageKey + "Data").html(data[messageKey]);

               }
          },
          error: function(passParams)
          {
               console.log(passParams)
          }
     });
}


function FetchPlayerInfo()
{
     console.log("Fetching Info");
     $.ajax({
          url: "/Home/FetchPlayerStats",
          type: "GET",
          success: function(data)
          {
               data = JSON.parse(data);
               console.log("Data");
               console.log(data);
               for (const messageKey in data)
               {
                    $("#" + messageKey + "Info").html(data[messageKey]);

               }
          },
          error: function(passParams)
          {
               console.log(passParams)
          }
     });
}

function FetchPlayerItems()
{
     console.log("Fetching items");
     $.ajax({
          url: "/Home/FetchPlayerItems",
          type: "GET",
          success: function(data)
          {
               data = JSON.parse(data);
               console.log("Data");
               console.log(data);
               for (const messageKey in data)
               {
                    console.log(messageKey);
                    console.log(data[messageKey]);
                    $("#" + messageKey + "Name").html(messageKey);
                    $("#" + messageKey + "Data").html(data[messageKey]);

               }
          },
          error: function(passParams)
          {
               console.log(passParams)
          }
     });
}
