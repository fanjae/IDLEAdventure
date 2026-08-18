using System;
//조건 검사
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
