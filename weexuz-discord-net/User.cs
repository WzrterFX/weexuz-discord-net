using System.Globalization;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Weexuz.Modules
{
    public class User : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("avatar", "View the avatar of the user.")]
        public async Task Avatar(SocketGuildUser member = null)
        {
            SocketGuildUser author = Context.User as SocketGuildUser;

            if (member == null) member = Context.User as SocketGuildUser;

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color(220, 20, 60))

                .WithTitle($"**<:weexuz_member:1227216765898199072> {member.DisplayName} avatar**")

                .WithImageUrl(member.GetAvatarUrl(ImageFormat.Auto, 2048) ?? member.GetDefaultAvatarUrl())

                .WithFooter($"Requested by {author.Username}", author.GetAvatarUrl());

            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("profile", "View the profile of the user.")]
        public async Task Profile(SocketGuildUser member = null)
        {
            SocketGuildUser author = Context.User as SocketGuildUser;

            if (member == null) member = Context.User as SocketGuildUser;

            string roles = string.Join(" ", member.Roles.Where(role => role.Name != "@everyone").Select(role => $"<@&{role.Id}>"));

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color(220, 20, 60))

                .WithTitle($"**<:weexuz_member:1227216765898199072> {member.DisplayName} profile**")

                .AddField("> ID", member.Id.ToString())
                .AddField("> Registered", $"{member.CreatedAt.ToUniversalTime().ToString("dd MMMM yyyy, HH:mm", CultureInfo.InvariantCulture)} UTC")
                .AddField("> Joined", $"{member.JoinedAt?.ToUniversalTime().ToString("dd MMMM yyyy, HH:mm", CultureInfo.InvariantCulture) ?? "Unknown"} UTC");

            if (!string.IsNullOrWhiteSpace(member.Nickname))
                embed.AddField("> Nickname", member.Nickname);

            if (!string.IsNullOrWhiteSpace(roles))
                embed.AddField("> Roles", roles);

            embed.WithThumbnailUrl(member.GetAvatarUrl() ?? member.GetDefaultAvatarUrl())

                .WithFooter($"Requested by {author.Username}", author.GetAvatarUrl());

            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("server", "View the server information.")]
        public async Task Server()
        {
            SocketGuildUser author = Context.User as SocketGuildUser;

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color(220, 20, 60))

                .WithTitle($"**<:weexuz_member:1227216765898199072> {Context.Guild.Name} information**")

                .AddField("> ID", Context.Guild.Id.ToString())
                .AddField("> Created on", $"{Context.Guild.CreatedAt.ToUniversalTime().ToString("dd MMMM yyyy, HH:mm", CultureInfo.InvariantCulture)} UTC")
                .AddField("> Owner", Context.Guild.Owner.ToString())
                .AddField("> Channels", $"{Context.Guild.TextChannels.Count} text channels \n{Context.Guild.VoiceChannels.Count} voice channels")
                .AddField("> Members", $"{Context.Guild.MemberCount} members")
                .AddField("> Roles", string.Join(" ", Context.Guild.Roles.Where(role => !role.IsEveryone).Select(role => $"<@&{role.Id}>")))

                .WithThumbnailUrl(Context.Guild.IconUrl)

                .WithFooter($"Requested by {author.Username}", author.GetAvatarUrl());

            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("channel", "View information about the current channel.")]
        public async Task Channel(SocketTextChannel channel = null)
        {
            SocketGuildUser author = Context.User as SocketGuildUser;

            SocketTextChannel text = channel ?? Context.Channel as SocketTextChannel;

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color(220, 20, 60))

                .WithTitle($"**<:weexuz_member:1227216765898199072> {text.Name} information**")

                .AddField("> ID", text.Id.ToString())
                .AddField("> Created on", $"{text.CreatedAt.ToString("dd MMMM yyyy, HH:mm", CultureInfo.InvariantCulture)} UTC")
                .AddField("> Topic", string.IsNullOrWhiteSpace(text.Topic) ? "None" : text.Topic)
                .AddField("> Category", $"{(text.Category?.Name ?? "None")}")

                .WithThumbnailUrl(Context.Guild.IconUrl)

                .WithFooter($"Requested by {author.Username}", author.GetAvatarUrl());

            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("role", "View information about a role.")]
        public async Task Role(SocketRole role)
        {
            SocketGuildUser author = Context.User as SocketGuildUser;

            string permissions = string.Join(", ", role.Permissions.ToList());

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color(220, 20, 60))

                .WithTitle($"**<:weexuz_member:1227216765898199072> {role.Name} information**")

                .AddField("> ID", role.Id.ToString())
                .AddField("> Created on", $"{role.CreatedAt.ToString("dd MMMM yyyy, HH:mm", CultureInfo.InvariantCulture)} UTC")
                .AddField("> Color", role.Color.ToString())
                .AddField("> Mentionable", role.IsMentionable ? "Yes" : "No")
                .AddField("> Hoisted", role.IsHoisted ? "Yes" : "No")
                .AddField("> Members", $"{role.Members.Count()} members");

            if (!string.IsNullOrWhiteSpace(permissions))
                embed.AddField("> Permissions", permissions);

            embed.WithThumbnailUrl(Context.Guild.IconUrl)

                .WithFooter($"Requested by {author.Username}", author.GetAvatarUrl());

            await RespondAsync(embed: embed.Build());
        }
    }
}