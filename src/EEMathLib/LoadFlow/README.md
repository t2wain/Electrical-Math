# Power Flow Analysis

Power flow analysis is based on the following equations and their derivations:

- I = V / Z = V * Y
- S = V * I.conjugate()
- S = V * (V * Y).conjugate()
- S = P + jQ
- P + jQ = V * (V * Y).conjugate()

Therfore, these two equations below are the basis of the load flow analysis 

- P = f(V,Y) * cos(A)
- Q = f(V,Y) * sin(A)

where

- Z : impedance
- Y : admittance
- V : voltage magnitude
- A : voltage phase angle
- P : real power
- Q : reactive power

Note, the paramater V, Y, and S are phasor quantity (magnitude, phase) which can be converted to complex value as (Real + jImaginary)

## Bus data

At each bus, the following data are specified.

For Slack bus:
- V = 1.0
- A = 0.0

For PV buses:
- V
- Pgen
- QgenMax
- QgenMin
- Pload
- Qload

For PQ buses:
- Pload
- Qload

## Power summation at each bus

The power delivered to each bus (Pk and Qk):
- Pk = Pgenk - Ploadk
- Qk = Qgenk - Qloadk

The sign (positive or negative) of the values for these parameters indicate the direction of power flow. The standard direction of power flow (positive sign) at each bus are:
- Pgen/Qgen flow into the bus
- Pload/Qload flow out of bus : expecting inductive Qload
- Pk/Qk flow out of bus


## Calculation of V, A, P, and Q at each bus

The power flow analysis is the calculation of parameters V, A, P, and Q at each bus. The buses in the network are classified according to which parameters are given at such bus

- Slack bus (utility bus) : given V = 1.0, A = 0.0, calculate P and Q
- PV bus (generator bus) : given V and P, calculate A and Q
- PQ bus (load bus) : given P and Q, calculate V and A

Once V and A are calculated for each bus, then the power flow for each line is calculated:
- Pline/Qline = f(Vfrom, Vto, Zline, Yline)
- Zline is the line series impedance
- Yline is the shunt admittance
- The sign value of Pline/Qline indicate the direction of power flow

## Power Flow Algorithms

There are two basic algorithm of load flow analysis:

- Gauss-Seidel
- Newton-Raphson

Both algorithms perform iterative calculations where the results of various parameters of previous iteration are used to calculate the new result in the current iteration until the difference of results between iterations is smaller than a specified threshold. The algorithm is **converging** if the value of such difference in successive iteration continues to shrink and eventually will be less than the specified threshold. However, if the difference is growing then the algorithm is **diverging** and so it will not be able to find a solution for this particular electrical network.

The strength of each algorithm is based on the following parameters:

- Number of iteration (how quickly it converge)
- Speed (number of calculation per iteration)
- Computer memmory size requirement
- Ability to converge (finding the solution)
- Accuracy vs. Approximation

Gauss-Seidel strength
- Require less computer memmory

Newton-Raphson strength
- Converge more often
- Require less number of iteration

Newton-Raphson Fast-Decoupled
- Require less computer memmory
- Speed (good)
- Approximation

Newton-Raphson DC-Like
- Require less computer memmory
- Speed (better)
- Approximation

## Code Design

The design of this code mostly follows the functional-programming architecture. The load flow algorithm requires many iterations and each iteration requires many calculation steps where the output of one calculation step is the input of the next calculation step. Much effort is made to ensure each calculation step can be tested independently and the final/intermediate outputs of each iteration and calculation step can be saved/inspected.