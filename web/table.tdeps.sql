CREATE TABLE TaskDeps (idtask bigint NOT NULL ,
iddep bigint NOT NULL ,
CONSTRAINT TaskDeps_pk_new PRIMARY KEY (idtask,iddep),
CONSTRAINT TaskDeps_fk_new FOREIGN KEY (idtask) REFERENCES public.tasks (id),
CONSTRAINT TaskDeps_fk_new FOREIGN KEY (iddep) REFERENCES public.tasks (id)
);
