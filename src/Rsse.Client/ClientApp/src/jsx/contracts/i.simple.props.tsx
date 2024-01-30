import { NoteResponseDto } from "../dto/note.response.dto.tsx";

export interface ISimpleProps {
    formId?: HTMLFormElement;
    jsonStorage?: NoteResponseDto;
    id?: string;
}
