# DataBindings

The databindings provide an easy, flexible and straightforward way of binding arbitrary fields of unity component fields to game logic / bound object fields.

## Concept

The core concept behind databinding derives from the WPF DataBindings.
Our implementation uses a tree-like structure built up in the unity scene graph.

### Roots

These represent the root of any databinding tree built up in the scene graph.
They are special kinds of nodes which don't have a parent.

### Nodes

Nodes in the databinding tree which bind to a specific field of a parent node (or root).
The node provides the field it binds to to all children bindings of this node.

### Leaves

There are different kinds of leaves:

#### Regular leaves

These leaves are just binding one node field to a specific field on an object.
The most prominent implementation is DataBindingGenericTemplatedLeaf.

#### Invokers

Invokers bind to a method on its parent.
They can be called via code or UnityEvents (UI Buttons, ...) to invoke their bound method.
Additionally parameter bindings can be established to bind to parameterized methods.

Invokers also implement node-behaviour and provide their method return type as bound type / object.
Every time the invoker will be accesses by a child databinding, the method is being invoked and the returned object is being passed to the child.

#### Collection leaves

Collections leaves can be used to implement collections where every element in the collection is being spawned as collection element prefab.
The collection element prefabs contain a root binding that is specifically implemented for collection elements, which is being assigned the item of the collection the element is created for.

## Implementation

### DataBindingRoot

Simple databinding root implementation that can bind to any arbitrary `UnityEngine.Object` type.

### DataBindingBranch

Databinding node implementation that will create a new branch on the databinding tree.
It will bind to one parent node and a field on it and then provide it to all children.

### DataBindingGenericTemplatedLeaf

The generic templated leaf is the most commonly used databinding leaf.
This leaf is composed of a template, the bind target object for the template and the leaf binding (parentNode / field).

The template is composed of a type and a field you want to bind to.
For example a template for a Unity UI Text field would set the type to the UnityEngine.UI.Text component.
The template field would be set to text.
This template would then bind to the text field of a Unity UI Text object.

On a leaf this template then is being assigned together with the template bind target (a Unity UI Text component).
Now every object from the parent nodes can be bound to the assigned Unity UI Text component's text!