set resultsPath=%1
set capFileName=%2


nmcap /network * /capture /File %resultsPath%\%capFileName%.chn /TerminateWhen /Frame "NbtNs.NbtNsQuestionSectionData.QuestionName.Name == 'NMTEST2 '"


@pause