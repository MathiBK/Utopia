
function GetNeighbor(hexX, hexY, direction)
{
    let directions = [{x: 1, y: 0}, {x: 1, y: -1}, {x: 0, y: -1}, {x: -1, y: 0}, {x: -1, y: 1}, {x: 0, y: 1}]
    let retNeigh = {x: (hexX + directions[direction].x), y: (hexY + directions[direction].y)}
    return retNeigh;
}

function Length(hexX, hexY)
{
    return Math.round(((Math.abs(hexX) + Math.abs(hexY) + Math.abs((-hexX)-hexY)) / 2));
}

function Distance(hexOneX, hexOneY, hexTwoX, hexTwoY)
{

    let newX = hexOneX-hexTwoX;
    let newY = hexOneY-hexTwoY;
    return Length(newX, newY);
}



function ConvertFromHexCoords(hexX, hexY, size) 
{
    let hexagonNarrow = 2 * size * 0.75;
    let hexagonHeight = size * Math.sqrt(3);
    let i = {x: hexagonNarrow, y: 0.5 * hexagonHeight};
    let  j = {x: 0, y: hexagonHeight};
    let coords = {x: 0, y:0}
    coords.x = i.x * hexX + j.x * hexY + size;
    coords.y = i.y * hexX + j.y * hexY;
    return coords;
}


//Bruk kartesianske hexkoordinater!!
function ComputeHexVerticesFromFace(cartesianHexX, cartesianHexY, size)
{
    let hexagonHeight = size * Math.sqrt(3);
    let vertices = []

    let fixY1= Math.round(((cartesianHexY + hexagonHeight / 2)+ Number.EPSILON) *100) / 100;
    let fixY2= Math.round(((cartesianHexY - hexagonHeight / 2) + Number.EPSILON)*100) / 100;
    let fixY3= Math.round((cartesianHexY + Number.EPSILON) * 100) / 100;

    let fixX1= Math.round(((cartesianHexX - size)+ Number.EPSILON) * 100) / 100;
    let fixX2= Math.round(((cartesianHexX + size)+ Number.EPSILON) * 100) / 100;
    let fixX3= Math.round(((cartesianHexX + size / 2)+ Number.EPSILON) * 100) / 100;
    let fixX4= Math.round(((cartesianHexX - size / 2)+ Number.EPSILON) * 100) / 100;


    let vertexL = {x: fixX1, y: fixY3};
    let vertexR = {x: fixX2, y: fixY3};


    let vertexTR = {x: fixX3, y: fixY1}
    let vertexTL = {x: fixX4, y: fixY1}

    let vertexBR = {x: fixX3, y: fixY2}
    let vertexBL = {x: fixX4, y: fixY2}

    vertices.push(vertexL);
    vertices.push(vertexBL);
    vertices.push(vertexBR);
    vertices.push(vertexR);
    vertices.push(vertexTR);
    vertices.push(vertexTL);

    return vertices;
}