using NoteEditor.DTO;
using NoteEditor.Notes;
using NoteEditor.Presenter;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NoteEditor.Model
{
    public class EditDataSerializer
    {
        public static string Serialize()
        {
            var dto = new MusicDTO.EditData();
            dto.BPM = EditData.BPM.Value;
            dto.maxBlock = EditData.MaxBlock.Value;
            dto.offset = EditData.OffsetSamples.Value;
            dto.name = Path.GetFileNameWithoutExtension(EditData.Name.Value);

            var sortedNoteObjects = EditData.Notes.Values
                .Where(note => !(note.note.type == NoteTypes.Long && EditData.Notes.ContainsKey(note.note.prev)))
                .OrderBy(note => note.note.position.ToSamples(Audio.Source.clip.frequency, EditData.BPM.Value));

            dto.notes = new List<MusicDTO.Note>();

            foreach (var noteObject in sortedNoteObjects)
            {
                //シングルノーツ
                if (noteObject.note.type == NoteTypes.Single)
                {
                    dto.notes.Add(ToDTO(noteObject));
                }
                //ロングノーツ
                else if (noteObject.note.type == NoteTypes.Long)
                {
                    var current = noteObject;
                    var note = ToDTO(noteObject);

                    while (EditData.Notes.ContainsKey(current.note.next))
                    {
                        var nextObj = EditData.Notes[current.note.next];
                        note.notes.Add(ToDTO(nextObj));
                        current = nextObj;
                    }

                    dto.notes.Add(note);
                }

              
                //アタックノーツ
                else if (noteObject.note.type == NoteTypes.Attack)
                {
                    dto.notes.Add(ToDTO(noteObject));
                }
                //コインノーツ
                else if (noteObject.note.type == NoteTypes.Coin)
                {
                    dto.notes.Add(ToDTO(noteObject));
                }

                //ジャンプノーツ
                else if (noteObject.note.type == NoteTypes.Jump)
                {
                    dto.notes.Add(ToDTO(noteObject));
                }

            }

            return UnityEngine.JsonUtility.ToJson(dto);
        }

        public static void Deserialize(string json)
        {
            var editData = UnityEngine.JsonUtility.FromJson<MusicDTO.EditData>(json);
            var notePresenter = EditNotesPresenter.Instance;

            EditData.BPM.Value = editData.BPM;
            EditData.MaxBlock.Value = editData.maxBlock;
            EditData.OffsetSamples.Value = editData.offset;

            foreach (var note in editData.notes)
            {
                switch (note.type)
                {
                    case 1: // Single
                    case 3: // Attack
                    case 4: // Coin
                    case 5: //Jump
                        notePresenter.AddNote(ToNoteObject(note));
                        break;

                    case 2: // Long
                        var longNoteObjects = new[] { note }.Concat(note.notes)
                            .Select(note_ =>
                            {
                                notePresenter.AddNote(ToNoteObject(note_));
                                return EditData.Notes[ToNoteObject(note_).position];
                            })
                            .ToList();

                        for (int i = 1; i < longNoteObjects.Count; i++)
                        {
                            longNoteObjects[i].note.prev = longNoteObjects[i - 1].note.position;
                            longNoteObjects[i - 1].note.next = longNoteObjects[i].note.position;
                        }
                        EditState.LongNoteTailPosition.Value = NotePosition.None;
                        break;
                }
            }
        }

        static MusicDTO.Note ToDTO(NoteObject noteObject)
        {
            var note = new MusicDTO.Note();
            note.num = noteObject.note.position.num;
            note.block = noteObject.note.position.block;
            note.LPB = noteObject.note.position.LPB;
            
            switch(noteObject.note.type)
            {
                //シングル
                case NoteTypes.Single:
                    note.type = 1;
                    break;
                //ロング
                case NoteTypes.Long:
                    note.type = 2;
                    break;
                //アタック
                case NoteTypes.Attack:
                    note.type = 3;
                    break;
                //コイン
                case NoteTypes.Coin:
                    note.type = 4;
                    break;
                //ジャンプ
                case NoteTypes.Jump:
                    note.type = 5;
                    break;
                default:
                    note.type = 1;
                    break;
            }

            note.notes = new List<MusicDTO.Note>();
            return note;
        }

        public static Note ToNoteObject(MusicDTO.Note musicNote)
        {
            NoteTypes type;

            switch(musicNote.type)
            {
                //シングル
                case 1:
                    type = NoteTypes.Single;
                    break;
                //ロング
                case 2:
                    type = NoteTypes.Long;
                    break;
                //アタック
                case 3:
                    type = NoteTypes.Attack;
                    break;
                //コイン
                case 4:
                    type = NoteTypes.Coin;
                    break;
                //ジャンプノーツ
                case 5:
                    type = NoteTypes.Jump;
                    break;
                default:
                    type = NoteTypes.Single; 
                    break;
            }

            return new Note(
                new NotePosition(musicNote.LPB, musicNote.num, musicNote.block), type);
              
        }
    }
}
