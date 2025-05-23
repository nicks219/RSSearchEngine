import {JSX, useContext} from "react";
import {NoteResponseDto} from "../dto/note.response.dto.tsx";
import {CommonContext} from "../common/context.provider";
import {toggleContainerVisibility} from "../common/visibility.handlers";
import {getCommonNoteId, getTextResponse, getTitleResponse} from "../common/dto.handlers";
import {Doms, Messages} from "../dto/doms.tsx";

export const ReadNote = (props: {formElement: HTMLFormElement|null, noteDto: NoteResponseDto}) => {
    const commonContext = useContext(CommonContext);
    const handleCheckboxesVisibility = () => {
        if (props.formElement) {
            props.formElement.style.display = toggleContainerVisibility(props.formElement.style.display);
        }
    }

    if (props.noteDto && Number(getCommonNoteId(props.noteDto)) !== 0) {
        commonContext.commonNumber = Number(getCommonNoteId(props.noteDto));
    }

    return (
        <span>
            {props.noteDto ? (getTextResponse(props.noteDto) ?
                    <span>
                    <div id={Doms.noteTitle} onClick={handleCheckboxesVisibility}>
                        {getTitleResponse(props.noteDto)}
                    </div>
                    <div id={Doms.noteText} onClick={handleCheckboxesVisibility}>
                        <TextSupportsLinks text={getTextResponse(props.noteDto) ?? ""}/>
                    </div>
                </span>
                    : Messages.selectTag)
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

    return <div className={Doms.userText}>{result}</div>
}
