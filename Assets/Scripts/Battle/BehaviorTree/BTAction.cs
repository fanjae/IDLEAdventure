using System;
//실제로 행동을 요청하는 노드
public class BTAction : BTNode
{
    private readonly Func<BTStatus> action;

    public BTAction(Func<BTStatus> action)
    {
        this.action = action;
    }
    public override BTStatus Run()
    {
        if (action == null)
        {
            return BTStatus.Failure;
        }
        return action();
    }
}
