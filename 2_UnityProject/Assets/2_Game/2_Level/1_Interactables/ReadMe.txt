

Every C# Class in here except for "Interactable" inherits from "Interactable", meaning is has all the same basic functionality that is explained.

Usage:

    Setup
        Utilize one of the predefined interaction types and attach them to any GameObject that requires interaction. 
        Ensure to add a collider to the GameObject, specifically set as a trigger. It's crucial to note that this is an additional collider. 
        Even if the object already possesses a collider, you must add this trigger collider and configure it accordingly. 
        This trigger collider defines the specific area where players can interact with the object.

    Setting Relationships
        Use the Unity Editor to assign triggeredBy and triggering variables for triggering relationships.
        When triggeredBy triggers, it invokes a Function depending on the type (For example the door opens the door).
        Trigger method recursively triggers the next object if triggering is not null.

    Visualizing Relationships:
            U can visualize the connection between two objects by clikcing one of them
            Red lines show the object triggering the current one (triggeredBy).
            Green lines show the object being triggered (triggering).

    Avoiding Loops:
        Be cautious not to create triggering loops to prevent unexpected behavior.
        A warning is issued if a loop is detected.