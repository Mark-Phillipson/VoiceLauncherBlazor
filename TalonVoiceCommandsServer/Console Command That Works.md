TalonStorageDB.loadCommandsChunked(1000).then(cmds => {
  const withTitle = cmds.filter(c => c.Title && c.Title.trim().length > 0);
  console.log("Commands with Title:", withTitle.length, withTitle.slice(0, 5));
});