import {useEffect, useReducer} from "react";
import {NoteResponseDto} from "../dto/request.response.dto";
import {toggleMenuVisibility} from "../common/visibility.handlers";
import {getTextResponse, getTitleResponse, setTextResponse} from "../common/dto.handlers";

export const UpdateNote = (props: {formElement?: HTMLFormElement, noteDto: NoteResponseDto}) => {
    const [, forceUpdate] = useReducer(x => x + 1, 0);

    useEffect(() => {
        getCookie();
    }, []);

    // именование кук ASP.NET: ".AspNetCore.Cookies": учитывая изменения в работе с куками со стороны браузера, вопрос: зачем?
    const getCookie = () => {
        // куки выставляются в компоненте Login:
        const name = "rsse_auth";
        const matches = document.cookie.match(new RegExp(
            "(?:^|; )" + name.replace(/([.$?*|{}()\[\]\\\/+^])/g, '\\$1') + "=([^;]*)"
        ));

        if (matches == null || decodeURIComponent(matches[1]) === 'false') {
            hideMenu();
        }
    }

    const hideMenu = () => {
        if (props.formElement) props.formElement.style.display = toggleMenuVisibility(props.formElement.style.display);
        (document.getElementById("login") as HTMLElement).style.display = "block";
    }

    const inputText = (e: string) => {
        setTextResponse(props.noteDto, e);
        forceUpdate();
    }

    return (
        <div>
            <p/>
            {props.noteDto != null ? (getTextResponse(props.noteDto) != null ?
                    <div>
                        <h1 onClick={hideMenu}>
                            {getTitleResponse(props.noteDto)}
                        </h1>
                        <h5>
                            <textarea name="msg" cols={66} rows={30} form="textbox"
                                      value={getTextResponse(props.noteDto)}
                                      onChange={e => inputText(e.target.value)}/>
                        </h5>
                    </div>
                    : "выберите заметку")
                : "loading.."}
        </div>
    );
}
