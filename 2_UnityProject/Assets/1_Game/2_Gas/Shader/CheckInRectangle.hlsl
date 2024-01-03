

void CheckInRectangle_float(float4 RectangleInfo,float2 Position, out float Out)
{	
    float2 rectangleCenter = float2(RectangleInfo.r, RectangleInfo.g);
    float halfWidth = RectangleInfo.b * 0.5;
    float halfHeight = RectangleInfo.a * 0.5;

    // Calculate the bounds of the rectangle
    float minX = rectangleCenter.x - halfWidth;
    float maxX = rectangleCenter.x + halfWidth;
    float minY = rectangleCenter.y - halfHeight;
    float maxY = rectangleCenter.y + halfHeight;

    // Check if the point is inside the rectangle
    bool isInside = Position.x >= minX && Position.x <= maxX && Position.y >= minY && Position.y <= maxY;

    // Output the result
    Out = isInside ? 1.0 : 0.0;
}
