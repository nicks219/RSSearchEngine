import { NoteResponseDto } from "../dto/request.response.dto.tsx";

export interface ISimpleProps {
    formId?: HTMLFormElement;
    jsonStorage?: NoteResponseDto;
    id?: string;
}

export interface IMountedComponent {
    mounted: boolean;
}

export interface ISubscribed<T> {
    subscription: T;
}

