using UnityEngine;

public class CommandExecuter : MonoBehaviour
{
    public static CommandExecuter instance;

    [SerializeField] private GameObject orangeUnitPrefab;
    [SerializeField] private GameObject orangeBigUnitPrefab;
    [SerializeField] private GameObject blueUnitPrefab;
    [SerializeField] private GameObject blueBigUnitPrefab;

    private void Generate(GenerateCommand cmd)
    {
        Vector3 generationPos = new(cmd.pos.x, 0, cmd.pos.y);
        GameObject unit = cmd.unitType switch
        {
            UnitType.OrangeSmall => orangeUnitPrefab,
            UnitType.OrangeBig => orangeBigUnitPrefab,
            UnitType.BlueSmall => blueUnitPrefab,
            UnitType.BlueBig => blueBigUnitPrefab,
            _ => null,
        };
        for (int i = 0; i < cmd.count; i++)
        {
            Instantiate(unit, generationPos, Quaternion.identity);
        }
    }

    private void OnCommandsReady(CommandsReadyEvent e)
    {
        foreach (var cmd in e.commands)
        {
            switch (cmd.commandType)
            {
                case CommandType.Generate:
                    Generate(cmd as GenerateCommand);
                    break;
                case CommandType.Move:
                    break;
                case CommandType.Destroy:
                    break;
            }
        }
    }

    private void Start()
    {
        instance = this;
        EventBus.Subscribe<CommandsReadyEvent>(OnCommandsReady);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<CommandsReadyEvent>(OnCommandsReady);
        instance = null;
    }
}
