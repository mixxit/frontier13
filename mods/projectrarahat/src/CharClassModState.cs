using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.GameContent;

namespace projectrarahat.src
{
    public sealed class CharClassModState
    {
        private static readonly CharClassModState instance = new CharClassModState();

        ConcurrentDictionary<string, bool> playerGrantedInitialItems = new ConcurrentDictionary<string, bool>();
        ConcurrentDictionary<string, CharacterClass> characterClasses = new ConcurrentDictionary<string, CharacterClass>();
        private CharClassModState() { }

        public CharacterClass GetCharacterClassByCode(string classCode)
        {
            if (this.characterClasses == null)
                return null;

            if (!this.characterClasses.ContainsKey(classCode))
                return null;

            return characterClasses[classCode];
        }

        public static CharClassModState Instance
        {
            get
            {
                return instance;
            }
        }

        public bool IsPlayerGrantedInitialItems(string playerUID)
        {
            return playerGrantedInitialItems.ContainsKey(playerUID);
        }

        public void SetPlayerGrantedInitialItems(string playerUID)
        {
            playerGrantedInitialItems[playerUID] = true;
        }

        internal ConcurrentDictionary<string, CharacterClass> GetCharacterClasses()
        {
            return this.characterClasses;
        }

        internal void SetCharacterClasses(List<CharacterClass> characterClasses)
        {
            this.characterClasses.Clear();
            foreach(var characterClass in characterClasses)
                this.characterClasses[characterClass.Code] = characterClass;
        }
    }
}
