import {FunctionComponentStateWrapper} from "./state.wrappers.tsx";

// TODO: данные контракты были нужны на этапе рефакторинга, сейчас следует избавиться от них

// export interface ISimpleProps {formElement?: HTMLFormElement; noteDto?: NoteResponseDto; id?: string;}
// export interface ISubscribedProps<T> {stateWrapper: T;}
// export interface IComplexProps extends ISimpleProps, ISubscribedProps<FunctionComponentStateWrapper<NoteResponseDto>> {}

// TODO: избавиться от данного "интрефейса" (хранилища глобального стейта): требуется ли явное "продолжение загрузки"?
// TODO: избавиться от <any> в FunctionComponentStateWrapper<any>: тип данных для смены стейта знает только компонент отображения и Loader:
// stateWrapperStorage/url выставляются в SetVisible<T>; noteIdStorage используется для передачи номера заметки между компонентами:

declare global {
    interface Window {
        noteIdStorage: number,
        stateWrapperStorage: FunctionComponentStateWrapper<any>,
        urlStorage: string
    }
}
