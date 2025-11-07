using System;
using System.Linq;
using Client;
using Client.Game.Data;
using Client.Networking;
using Client.Networking.Arguments;
using Client.Networking.Packets;

namespace TazUOGodot.g_src;

public class EventListeners
{
    internal EventListeners()
    {
        ShardListEventArgs.Update += ShardListEventArgsOnUpdate;
        CharacterListEventArgs.Update += CharacterListEventArgsOnUpdate;
    }

    private void CharacterListEventArgsOnUpdate(CharacterListEventArgs e)
    {
        CharInfo[]? characterList = e.Characters?.ToArray();
        if (characterList == null || characterList.Length == 0)
        {
            Logger.LogError($"{nameof(Assistant)}: No characters to select.");
            e.State.Detach();
            return;
        }

        CharInfo firstCharacter = characterList[0];

        Logger.Log("----> Selecting character: " + firstCharacter.Name);
        
        firstCharacter.Play();
        Network.State?.Slice();
    }

    private void ShardListEventArgsOnUpdate(ShardListEventArgs e)
    {
        if (e.ShardEntries == null || e.ShardEntries.Length == 0)
        {
            Logger.PushWarning("No shard entries found.");
            return;
        }
        
        //Show server select here normally, for now just select first
        Logger.Log($"--> Connecting to: {e.ShardEntries[0].Name}");
        e.ShardEntries[0].SendSelectPacket();
        Network.State?.Slice();
    }
}