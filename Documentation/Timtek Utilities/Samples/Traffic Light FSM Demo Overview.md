# Traffic Light FSM Demo Overview

Purpose
- Demonstrate the use of the TA.Utils.Core FiniteStateMachine<TState> with a realistic, visual example.
- Showcase clean separation between state behaviour (IState), triggering actions (ITrafficLightActions), and orchestration (IApproachSequencer), following SOLID and MVVM.
- Provide a deterministic, testable simulation using .NET 8 TimeProvider.

What the demo simulates
- An n‑way controlled area with one traffic light (approach) per entry.
- Each traffic light runs an independent FSM modelling phases as states: InactiveRed → RedAmber → Green → Amber → ClearingRed → InactiveRed. Fault is modelled as a flashing Amber state with no runtime exit.
- A sequencer that selects which approach is active using a round‑robin policy (single active approach in the initial implementation).
- Traffic flow via arrivals and departures per approach with configurable rates.

How it works (high level)
- On Start, the app starts the arrival and departure simulations, starts the sequencer, then starts each approach’s FSM after a random startup delay (up to 10 s). The sequencer waits until all lights report InactiveRed before sequencing.
- The sequencer activates one approach at a time by sending SetActiveApproach(config), which supplies per‑activation timings (ActiveTime, TransitionTime, ClearingTime). The light runs its FSM to completion and returns to InactiveRed, signalling the sequencer to select the next approach.
- While an approach is in Green, the sequencer may resend SetActiveApproach(config) to reset and adjust the dwell time from the moment of receipt.
- Stop sends StopTraffic to all approaches and waits for all to reach InactiveRed; simulations stop when the sequencer stops. Reset performs a full restart to recover from Fault.

UI overview
- Approach tiles show a visual traffic light, current state name, and time remaining.
- A full‑width chart plots queue length over time, one coloured line per approach.
- Controls:
  - Start/Stop/Reset (IRelayCommand‑bound, auto‑disabled while executing)
  - Number of approaches (editable only while stopped)
  - Timing and simulation inputs: ActiveTime, TransitionTime, ClearingTime; arrival mean (1–300 s); departure interval (1.0–10.0 s)
- Telemetry formatting uses fixed width with signed values per house style.

Traffic model
- Arrivals: stochastic process with configurable mean inter‑arrival time (recommended exponential distribution), one RNG stream per approach.
- Departures (active approach): deterministic interval; departures are eligible during Green and the first half of Amber (TransitionTime/2) only.
- Queue length updates on every arrival and departure; the chart updates in real time with efficient sampling.

Design principles highlighted
- Interface Segregation (ITrafficLightState vs ITrafficLightActions)
- Dependency Injection (Ninject CompositionRoot)
- Determinism and testability (TimeProvider‑driven timers; MSpec BDD tests)
- Clear orchestration vs behaviour separation (Sequencer vs Light FSM)

What to explore next
- Alternative sequencing strategies (e.g., favour longest queue, sensor‑driven)
- Multi‑active compatibility groups
- Fault detection and reporting
- Persistence of configurations and session replay