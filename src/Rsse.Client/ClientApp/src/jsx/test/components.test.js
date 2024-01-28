import { render, screen } from '@testing-library/react';
import React from 'react';
import MenuRouter from "../menu/menu.router.tsx";

describe('render menu component', () => {
    it("first", () => {
        // arrange:
        render(<MenuRouter/>);
        // act:
        const menuElement = screen.getByText("Посмотреть");
        // assert:
        expect(menuElement).toBeInTheDocument();
    });
});
