import {useReducer} from "react";
import {NoteResponseDto} from "../dto/request.response.dto";
import {getTextResponse, getTitleResponse, setTextResponse, setTitleResponse} from "../common/dto.handlers";

export const CreateNote = (props: {noteDto: NoteResponseDto}) => {
    const [, forceUpdate] = useReducer(x => x + 1, 0);

    // TODO: напиши новый вариант ввода, без апдейта на каждый символ:
    const textHandler = (e: string) => {
        setTextResponse(props.noteDto, e);
        forceUpdate();
    }

    // TODO: напиши новый вариант ввода, без апдейта на каждый символ:
    const titleHandler = (e: string) => {
        setTitleResponse(props.noteDto, e);
        forceUpdate();
    }

    return (
        <div>
            <p/>
            {props.noteDto ?
                <div>
                    <h5>
                        <textarea name="ttl" cols={66} rows={1} form="textbox"
                                  value={getTitleResponse(props.noteDto)}
                                  onChange={e => titleHandler(e.target.value)}/>
                    </h5>
                    <h5>
                        <textarea name="msg" cols={66} rows={30} form="textbox"
                                  value={getTextResponse(props.noteDto)}
                                  onChange={e => textHandler(e.target.value)}/>
                    </h5>
                </div>
                : "loading.."}
        </div>
    );
}
