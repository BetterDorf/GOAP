using System.Collections.Generic;
using UnityEngine;

namespace GOAP
{
    public class EnemyAI : Character
    {
        [SerializeField] ForwardIDA planner;

        [SerializeField] WorldState goal;
        WorldState currentInformation;

        List<GOAPNode> currentPlan;

        GameDirector gameDirector;

        // Replan the current plan after performing x actions of it
        [SerializeField] int replanAfterX;
        int planProgress = 0;

        private void Start()
        {
            currentPlan = new List<GOAPNode>();
            currentInformation = new WorldState();
            planner.ChangeGoal(goal, -1);
        }

        public void SetGameDirector(GameDirector gd)
        {
            gameDirector = gd;
        }

        public override void PlayTurn()
        {
            if (currentPlan == null || planProgress >= currentPlan.Count || planProgress >= replanAfterX)
            {
                GatherInformation();
                currentPlan = planner.Plan(currentInformation, goal);
                planProgress = 0;
            }

            if (currentPlan != null)
            {
                ExectuteAction(currentPlan[planProgress].ActionId);
                planProgress += 1;
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
        }

        void ExectuteAction(int id)
        {

        }
    }
}