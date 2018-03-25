# BehaviourModel

Provides pre-built components and a framework that can be used to create models of object behaviour and implement this behaviour in an abstract way.
Using this pattern has the advantage of writing every game code modular and abstract.
The main "Behaviour Model" does not know about any functionality that is being implemented, instead it only knows the functionality it will be providing.
Components can then bind to activities, events, etc. and provide the actual implementation of the functionality the behaviour model implements.

## Components

### Activity

An activity can be started and stopped. A start or stop can fail.
An example for an activity would be running in a first person shooter.
When the player presses the run button, the activity is being started and its being stopped when the button is being released.

### AttemptEvent

A specialized event implementation that can be used to attempt something.
An example use case for this would be firing a weapon, when the user presses the fire button, an attempt on a fire AttemptEvent is made.
If the weapon can fire, it will fire and if not it will not and let the callee know about it.

### MessageEvent

Simple message event, just a very simple regular event implementation.

### ModifiableValue

Implements a datatype that can be used as a field that is assignable from unity editor and overridable / modifyable via code.

### ModelProperty

Implements a property with getter / setter. This comes with the limitation that the getter can only be set to one method, unlike the other events.
Setting however can be implemented by multiple set event recievers.

### ModelCollectionProperty

Similar to ModelProperty but provides the ability to concat collections returned from the getters.
This is useful in situations where you have for example an inventory mechanic property that lists all items but the items are held by multiple logic components (for example multiple bags).
Setters can consume objects from the set call in order to claim them being set on themselves.

## Framework

Provides a very simple set of base classes that can be extended to make use of the behaviour model components.

### BehaviourModel

A behaviour model is the base component of the behaviour model framework and acts as hub for the parts of the behaviour model (the mechanics).

### BeahviourModelMechanic

Base class for implementing behaviour model mechanics, which are defining behaviour using the behaviour model components.
For example a player behaviour model could have its behaviour model component and another movement mechanic component (which derives from BehaviourModelMechanic).
BehaviourModelMechanicComponents can be used to set up the components for this mechanic.

### BehaviourModelMechanicComponent

Base class for implementing mechanic components.
