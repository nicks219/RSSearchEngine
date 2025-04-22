import {useEffect, useReducer} from "react";
import {NoteResponseDto} from "../dto/request.response.dto";
import {toggleContainerVisibility} from "../common/visibility.handlers";
import {getTextResponse, getTitleResponse, setTextResponse} from "../common/dto.handlers";
import {Doms, Messages, SystemConstants} from "../dto/doms.tsx";

export const UpdateNote = (props: {formElement?: HTMLFormElement, noteDto: NoteResponseDto}) => {
    const textAreaCols: number = 73;
    const textAreaRows: number = 30;

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
        if (props.formElement) props.formElement.style.display = toggleContainerVisibility(props.formElement.style.display);
        (document.getElementById(Doms.loginName) as HTMLElement).style.display = SystemConstants.block;
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
                            <textarea name={Doms.msg} cols={textAreaCols} rows={textAreaRows} form={Doms.textbox}
                                      value={getTextResponse(props.noteDto)}
                                      onChange={e => inputText(e.target.value)}/>
                        </h5>
                    </div>
                    : Messages.selectNote)
                : "loading.."}
        </div>
    );
}
