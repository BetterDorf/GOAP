using System;
using System.Collections.Generic;
using UnityEngine;

namespace GOAP
{
    public class EnemyAI : Character
    {
        [SerializeField] ForwardIDA planner;
        [SerializeField] ReverseIDA reversePlanner;
        [SerializeField] List<SimulAction> actions;
        [SerializeField] int restValue;
        [SerializeField] int kickValue;
        [SerializeField] int kickEnergy;
        [SerializeField] int punchValue;
        [SerializeField] int punchEnergy;
        [SerializeField] int shootValue;
        [SerializeField] int shootEnergy;

        [SerializeField] WorldState goal;
        WorldState currentInformation;

        [SerializeField] bool FSMMode = false;
        [SerializeField] bool reverseIDA = false;

        List<GOAPNode> currentPlan;

        GameDirector gameDirector;

        // Replan the current plan after performing x actions of it
        [SerializeField] int replanAfterX;
        int planProgress = 0;

        bool skipTurn = false;

        private void Start()
        {
            currentPlan = new List<GOAPNode>();
            currentInformation = new WorldState();
            planner.ChangeGoal(goal, -1);
            reversePlanner.ChangeGoal(goal, -1);

            // Setup actions
            actions[0].action = Move;
            actions[1].action = Rest;
            actions[2].action = Kick;
            actions[3].action = Punch;
            actions[4].action = Shoot;

            for (int i = 0;i< actions.Count; i++)
            {
                planner.AddAction(new GOAP.Action(actions[i], i));
                reversePlanner.AddAction(new GOAP.Action(actions[i], i));
            }
        }

        public void SetGameDirector(GameDirector gd)
        {
            gameDirector = gd;
        }

        public override void PlayTurn()
        {
            if (skipTurn)
            {
                skipTurn = false;
                return;
            }

            if (FSMMode)
            {
                GatherInformation();
                FSMLogic();
            }
            else
            {
                if (currentPlan == null || planProgress >= currentPlan.Count || planProgress >= replanAfterX)
                {
                    GatherInformation();
                    if (reverseIDA)
                    {
                        reversePlanner.SetInitialWorld(currentInformation);
                        currentPlan = reversePlanner.Plan(currentInformation, goal);
                    }
                    else
                    {
                        currentPlan = planner.Plan(currentInformation, goal);
                    }
                    planProgress = 1;
                }

                if (currentPlan != null)
                {
                    int i = planProgress;
                    if (reverseIDA)
                    {
                        i = currentPlan.Count - i - 1;
                    }

                    ExectuteAction(currentPlan[planProgress].ActionId);
                    planProgress += 1;
                }
            }
        }

        public void GatherInformation()
        {
            
            foreach (var node in gameDirector.CharaterPositions())
            {
                currentInformation.SetValue("selfHealth", health);
                currentInformation.SetValue("selfEnergy", energy);
                if (node.Character == this.gameObject)
                {
                    currentInformation.SetValue("selfPos", node.Id);
                }
                else
                {
                    currentInformation.SetValue("otherPos", node.Id);
                    var other = node.Character.GetComponent<Character>();
                    currentInformation.SetValue("otherHealth", other.health);
                }
            }

            currentInformation.SetValue("distance", Math.Abs( currentInformation.state["selfPos"] - currentInformation.state["otherPos"]));
        }

        void FSMLogic()
        {
            if (energy <= 0)
            {
                if (currentInformation.state["distance"] > 2)
                {
                    Move();
                }
                else
                {
                    Rest();
                }
            }
            else if (energy > 0 && energy < kickEnergy)
            {
                if (currentInformation.state["distance"] == 1)
                {
                    Punch();
                }
                else if (currentInformation.state["distance"] == 2)
                {
                    Move();
                }
                else if (energy > 1)
                {
                    Shoot();
                }
                else
                {
                    Rest();
                }
            }
            else
            {
                if (currentInformation.state["distance"] == 1)
                {
                    Kick();
                }
                else
                {
                    Shoot();
                }
            }
        }

        void ExectuteAction(int id)
        {
            actions[id].action();
        }

        void Move()
        {
            if (gameDirector.arena[currentInformation.state["selfPos"] + 1].Character == null)
            {
                gameDirector.arena[currentInformation.state["selfPos"] + 1].Character = this.gameObject;
                gameDirector.arena[currentInformation.state["selfPos"]].Character = null;

                transform.position = gameDirector.arena[currentInformation.state["selfPos"] + 1].Position;
            }
        }

        void Rest()
        {
            energy += restValue;
            Debug.Log("Rested for " + restValue);
        }

        void Kick()
        {
            if (currentInformation.state["distance"] <= 1 && energy >= kickEnergy)
            {
                gameDirector.arena[currentInformation.state["otherPos"]].Character.GetComponent<Character>().TakeDamage(kickValue);
                energy -= kickEnergy;
                skipTurn = true;
                Debug.Log("Kicked");
            }
        }

        void Punch()
        {
            if (currentInformation.state["distance"] <= 1 && energy >= punchEnergy)
            {
                gameDirector.arena[currentInformation.state["otherPos"]].Character.GetComponent<Character>().TakeDamage(punchValue);
                energy -= punchEnergy;
                Debug.Log("Punched");
            }
        }

        void Shoot()
        {
            if (currentInformation.state["distance"] > 1 && energy >= shootEnergy)
            {
                gameDirector.arena[currentInformation.state["otherPos"]].Character.GetComponent<Character>().TakeDamage(shootValue);
                energy -= shootEnergy;
                Debug.Log("shot");
            }
        }
    }
}