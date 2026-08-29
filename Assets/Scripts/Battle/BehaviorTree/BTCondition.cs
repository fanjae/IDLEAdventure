using System;
//현재 상황을 확인하는 노드
public class BTCondition : BTNode
{
    private readonly Func<bool> condition;

    public BTCondition(Func<bool> condition)
    {
        this.condition = condition;
    }
    public override BTStatus Run()
    {
        if (condition == null)
        {
            return BTStatus.Failure;
        }
        return condition() ? BTStatus.Success : BTStatus.Failure;
    }
}
