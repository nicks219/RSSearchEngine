import {useContext} from "react";
import {NoteResponseDto} from "../dto/request.response.dto";
import {CommonContext} from "../common/context.provider";
import {toggleMenuVisibility} from "../common/visibility.handlers";
import {getCommonNoteId, getTextResponse, getTitleResponse} from "../common/dto.handlers";

export const ReadNote = (props: {formElement?: HTMLFormElement, noteDto: NoteResponseDto}) => {
    const commonContext = useContext(CommonContext);
    const hideMenu = () => {
        if (props.formElement) {
            props.formElement.style.display = toggleMenuVisibility(props.formElement.style.display);
        }
    }

    if (props.noteDto && Number(getCommonNoteId(props.noteDto)) !== 0) {
        commonContext.commonNumber = Number(getCommonNoteId(props.noteDto));
    }

    return (
        <span>
            {props.noteDto ? (getTextResponse(props.noteDto) ?
                    <span>
                    <div id="noteTitle" onClick={hideMenu}>
                        {getTitleResponse(props.noteDto)}
                    </div>
                    <div id="noteText">
                        <TextSupportsLinks text={getTextResponse(props.noteDto) ?? ""}/>
                    </div>
                </span>
                    : "select tag please")
                : ""}
        </span>
    );
}

const TextSupportsLinks = (props: {text: string}): JSX.Element => {
    const result: (string | JSX.Element)[] = [];
    // https://css-tricks.com/almanac/properties/o/overflow-wrap/#:~:text=overflow%2Dwrap%20is%20generally%20used,%2C%20and%20Korean%20(CJK).
    props && props.text.replace(
        /((?:https?:\/\/|ftps?:\/\/|\bwww\.)(?:(?![.,?!;:()]*(?:\s|$))[^\s]){2,})|(\n+|(?:(?!(?:https?:\/\/|ftp:\/\/|\bwww\.)(?:(?![.,?!;:()]*(?:\s|$))[^\s]){2,}).)+)/gim,
        (_: string, link: string, text: string): string => {
            result.push(link ? <a href={(link[0] === "w" ? "//" : "") + link} key={result.length}>{link}</a> : text);
            return "";
        })

    return <div className="user-text">{result}</div>
}
