using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace HouraiTeahouse.FantasyCrescendo {

public class GameView : IInitializable<MatchConfig>, IStateView<MatchState> {

  public IStateView<MatchState>[] MatchViews;
  public PlayerView[] PlayerViews;

  public async Task Initialize(MatchConfig config) {
    var playerInit = InitializePlayers(config);
    var otherInit = InitializeOtherViews(config);
    await Task.WhenAll(playerInit, otherInit);
  }

  async Task InitializePlayers(MatchConfig config) {
    PlayerViews = new PlayerView[config.PlayerCount];
    var tasks = new List<Task>();
    var viewFactories = Object.FindObjectsOfType<AbstractViewFactory<PlayerState, PlayerConfig>>();
    for (int i = 0; i < PlayerViews.Length; i++) {
      PlayerViews[i] = new PlayerView(viewFactories);
      tasks.Add(PlayerViews[i].Initialize(config.PlayerConfigs[i]));
    }
    await Task.WhenAll(tasks);
  }

  async Task InitializeOtherViews(MatchConfig config) {
    var factories = Object.FindObjectsOfType<AbstractViewFactory<MatchState, MatchConfig>>();
    var viewsTask = await Task.WhenAll(factories.Select(f => f.CreateViews(config)));
    MatchViews = viewsTask.SelectMany(v => v).ToArray();
  }

  public void ApplyState(MatchState state) {
    ApplyPlayerStates(state);
    ApplyOtherStates(state);
  }

  void ApplyPlayerStates(MatchState state) {
    for (int i = 0; i < PlayerViews.Length; i++) {
      PlayerViews[i].ApplyState(state.PlayerStates[i]);
    }
  }

  void ApplyOtherStates(MatchState state) {
    foreach (var view in MatchViews) {
      view.ApplyState(state);
    }
  }

}

}